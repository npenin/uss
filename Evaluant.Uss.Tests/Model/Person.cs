using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Tests.Model
{
    public class Person
    {
        public Person()
        {
            Id = Guid.NewGuid();
            Friends = new List<Person>();
            Cars = new List<Car>();
        }

        public Guid Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual Address Address { get; set; }

        public virtual DateTime? BirthDate { get; set; }

        public IList<Person> Friends { get; set; }

        public IList<Car> Cars { get; set; }
    }
}
