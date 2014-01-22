using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaluant.Uss.Tests.Model
{
    public class Car
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual Company Constructor { get; set; }
    }
}
