using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.EntityResolver
{
#if !SILVERLIGHT
    [global::System.Serializable]
#endif
    public class EntityResolverException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EntityResolverException() { }
        public EntityResolverException(string message) : base(message) { }
        public EntityResolverException(string message, Exception inner) : base(message, inner) { }
#if !SILVERLIGHT
        protected EntityResolverException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#endif
    }
}
