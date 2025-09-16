using System;

namespace SMTPRouter
{
    /// <summary>
    /// Represents an exception thrown when the <see cref="Router"/> could not create the queues
    /// </summary>
    public sealed class QueuesNotInitializedException: Exception
    {
        private const string DEFAULT_ERROR_MESSAGE = "Unable to initialize queues";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuesNotInitializedException"/>
        /// </summary>
        public QueuesNotInitializedException(): this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuesNotInitializedException"/>
        /// </summary>
        /// <param name="innerException">The exception that caused the queue to not be initialized</param>
        public QueuesNotInitializedException(Exception? innerException): base(QueuesNotInitializedException.DEFAULT_ERROR_MESSAGE, innerException)
        {

        }

        
    }
}
