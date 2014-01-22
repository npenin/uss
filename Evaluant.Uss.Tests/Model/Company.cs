using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Tests.Model
{
    public class Company
    {
        public Company()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public virtual Address Address { get; set; }

        public string Name { get; set; }
    }
}
