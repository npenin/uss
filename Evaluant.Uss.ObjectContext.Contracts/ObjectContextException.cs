using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.ObjectContext.Contracts
{
#if !SILVERLIGHT
    [global::System.Serializable]
#endif
    public class ObjectContextException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ObjectContextException() { }
        public ObjectContextException(string message) : base(message) { }
        public ObjectContextException(string message, Exception inner) : base(message, inner) { }
#if !SILVERLIGHT
        protected ObjectContextException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#endif
    }
}
