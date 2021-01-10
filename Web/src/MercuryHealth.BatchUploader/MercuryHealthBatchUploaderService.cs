using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;

namespace MercuryHealth.BatchUploader
{
    public partial class MercuryHealthBatchUploaderService : ServiceBase
    {
        private Timer _timer = new Timer();
        private string _connectionString = "Server=OnPremServer;Database=MercuryHealthDB;Trusted_Connection=True;";
        private string _uploadShare = "";
        private bool _debug = false;
        private Timer _cleanupTimer = new Timer();

        public MercuryHealthBatchUploaderService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["connectionString"];
            _uploadShare = System.Configuration.ConfigurationManager.AppSettings["uploadFolder"];
            _debug = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["debug"]);

            WriteToFile("Service is started at " + DateTime.Now);
            WriteToFile("ConnectionString: " + _connectionString);
            WriteToFile("Upload Share: " + _uploadShare);
            WriteToFile("Debug: " + _debug);

            _timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            _timer.Interval = 5000; //number in milisecinds  
            _timer.Enabled = true;

            _cleanupTimer.Elapsed += new ElapsedEventHandler(OnCleanupTime);
            _cleanupTimer.Interval = 15000; //number in milisecinds  
            _cleanupTimer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }


        private void OnCleanupTime(object source, ElapsedEventArgs e)
        {

            System.IO.DirectoryInfo di = new DirectoryInfo(_uploadShare);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    String sqlCommandString = "select * from Exercises";
                    SqlCommand command = new SqlCommand(sqlCommandString, connection);
                    List<Exercise> exerciseList = new List<Exercise>();
                    Exercise newExercise;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                newExercise = new Exercise();
                                newExercise.Id = reader.GetGuid(reader.GetOrdinal("Id")).ToString();
                                newExercise.Name = reader.GetString(reader.GetOrdinal("Name")).ToString();
                                newExercise.MusclesInvolved = reader.GetString(reader.GetOrdinal("MusclesInvolved")).ToString();
                                newExercise.VideoUrl = reader.GetString(reader.GetOrdinal("VideoUrl")).ToString();
                                newExercise.Equipment = reader.GetString(reader.GetOrdinal("Equipment")).ToString();

                                exerciseList.Add(newExercise);
                            }
                        }
                    }

                    WriteToFile("    Exercise List Size: " + exerciseList.Count);
                    this.WriteUploadBatchFile(exerciseList);
                }
            }
            catch(Exception ex)
            {
                WriteToFile("    Exception: " + ex.Message);
            }
            
        }

        private void WriteUploadBatchFile(List<Exercise> exerciseList)
        {
            WriteToFile("    In WriteUploadBatchFile: " + exerciseList.Count);
            if (!Directory.Exists(_uploadShare))
            {
                Directory.CreateDirectory(_uploadShare);
            }
            string filepath =  _uploadShare + "\\" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    foreach(var exercise in exerciseList)
                    {
                        sw.WriteLine(exercise.ToString());
                    }
                    
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    foreach (var exercise in exerciseList)
                    {
                        sw.WriteLine(exercise.ToString());
                    }
                }
            }
        }

        private void WriteToFile(string message)
        {
            if (_debug)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(message);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(message);
                    }
                }
            }
            
        }
    }
}
