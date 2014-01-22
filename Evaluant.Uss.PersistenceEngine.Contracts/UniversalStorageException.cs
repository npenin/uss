using System;
#if	!SILVERLIGHT
using System.Runtime.Serialization;
#endif

namespace Evaluant.Uss
{
	/// <summary>
	/// Description résumée de UniversalStorageException.
	/// </summary>
	
#if	!SILVERLIGHT
    [Serializable]
#endif
	public class UniversalStorageException : Exception
	{
        public UniversalStorageException(string message)
            : base(message)
        {
        }

        public UniversalStorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if !SILVERLIGHT
        protected UniversalStorageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
