using System;
using System.Runtime.Serialization;

namespace Evaluant.Uss
{
	/// <summary>
	/// Base classe for all exceptions in Evaluant.Uss namespace
	/// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class PersistenceEngineException :
#if !SILVERLIGHT
        ApplicationException
#else
        Exception
#endif
	{
		public PersistenceEngineException() : base()
		{
		}

		public PersistenceEngineException(string message) : base(message)
		{
		}

		public PersistenceEngineException(string message, Exception inner) : base(message, inner)
		{
		}
#if !SILVERLIGHT
        protected PersistenceEngineException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
#endif
    }
}
