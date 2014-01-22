using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Tests.Model
{
    public class Employee : Person
    {
        public Employee()
        {
        }

        public virtual Company Company { get; set; }
    }
}
