using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaluant.Uss.Tests.Engines.SqlMapper.Mappings.Composite
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthday { get; set; }
        public virtual Address Address { get; set; }
    }
}
