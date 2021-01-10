using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryHealth.BatchUploader
{
    class Exercise
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public string MusclesInvolved { get; set; }
        public string Equipment { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Id);
            builder.Append("; ");
            builder.Append(Name);
            builder.Append("; ");
            builder.Append(Description);
            builder.Append("; ");
            builder.Append(VideoUrl);
            builder.Append("; ");
            builder.Append(MusclesInvolved);
            builder.Append("; ");
            builder.Append(Equipment);

            return builder.ToString();

        }

    }

    
}
