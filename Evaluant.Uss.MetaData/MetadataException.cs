using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.MetaData
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class MetadataException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MetadataException() { }
        public MetadataException(string message) : base(message) { }
        public MetadataException(string message, Exception inner) : base(message, inner) { }
#if !SILVERLIGHT
        protected MetadataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#endif
    }
}
