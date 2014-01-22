using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaluant.Uss.Tests.Engines.SqlMapper.Mappings.Composite
{
    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
    }
}
