using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Evaluant.Uss.Utility
{
    public class PropertyChangingEventArgs : CancelEventArgs
    {
        public PropertyChangingEventArgs(string name)
        {
            PropertyName = name;
        }

        public string PropertyName { get; private set; }
    }
}
