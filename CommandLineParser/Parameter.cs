using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineParser
{
    public class Parameter
    {
        public Parameter(string fullName, string shortName, bool mandatory, string help)
            : this(fullName)
        {
            this.Name = shortName;
            this.Help = help;
            this.Mandatory = mandatory;
        }
        public Parameter(string fullName, string shortName, string help)
            : this(fullName, shortName, false, help)
        {

        }

        public Parameter(string fullName)
        {
            this.FullName = fullName;
        }

        public string Name { get; set; }

        public string FullName { get; set; }

        public bool Mandatory { get; set; }

        public string Help { get; set; }
    }
}
