using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.MongoDB
{
    /// <summary>
    /// Raised when an action causes a unique constraint violation in an index. 
    /// </summary>
    public class MongoDuplicateKeyException : MongoOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDuplicateKeyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="error">The error.</param>
        public MongoDuplicateKeyException(string message, Entity error) : base(message, error, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDuplicateKeyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="error">The error.</param>
        /// <param name="e">The e.</param>
        public MongoDuplicateKeyException(string message, Entity error, Exception e) : base(message, error, e) { }
    }
}