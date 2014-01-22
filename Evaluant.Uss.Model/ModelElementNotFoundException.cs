using System;
using System.Text;

namespace Evaluant.Uss.Model
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ModelElementNotFoundException : 
        #if !SILVERLIGHT
ApplicationException
#else
Exception
#endif
    {
        public ModelElementNotFoundException() : base()
        {
        }

        public ModelElementNotFoundException(string message) : base(message)
        {
        }
    }
}
