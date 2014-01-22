using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.MongoDB
{
    public class MongoOperationException : MongoException
    {
        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>The error.</value>
        public Entity Error { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoOperationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="error">The error.</param>
        public MongoOperationException(string message, Entity error):this(message, error,null){}

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoOperationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="error">The error.</param>
        /// <param name="e">The e.</param>
        public MongoOperationException(string message, Entity error, Exception e)
            : base(message, e)
        {
            this.Error = error;
        }        
    }
}