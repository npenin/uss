using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Utility
{
    /// <summary>
    /// Represents a class which is able to create instances of a specific type
    /// <seealso cref="Evaluant.Uss.Common.ActivatorFactory"/>
    /// </summary>
    public interface IActivator
    {
        object CreateInstance();
    }
}
