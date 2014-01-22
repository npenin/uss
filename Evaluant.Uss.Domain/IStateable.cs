using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Domain
{
    public interface IStateable
    {
        State State { get; set; }
    }
}
