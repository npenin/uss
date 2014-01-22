using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaluant.Uss.Tests.ObjectContext.Relationships.Model
{
    public class Person
    {
        public Person()
        {
            Addresses = new List<Address>();
        }

        public string Name { get; set; }

        public IList<Address> Addresses { get; set; }
        public virtual Pet Pet { get; set; }
    }
}
