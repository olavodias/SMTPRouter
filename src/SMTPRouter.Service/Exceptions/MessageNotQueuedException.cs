using SMTPRouter.Models;
using System;

namespace SMTPRouter
{
    /// <summary>
    /// Represents an exception that is thrown when a message could not be added to the queue by the <see cref="Router"/>
    /// </summary>
    public sealed class MessageNotQueuedException: RoutableMessageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotQueuedException"/>
        /// </summary>
        public MessageNotQueuedException() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotQueuedException"/>
        /// </summary>
        /// <param name="routableMessage">The Message that could not be queued</param>
        public MessageNotQueuedException(RoutableMessage? routableMessage) : this(routableMessage, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageNotQueuedException"/>
        /// </summary>
        /// <param name="routableMessage">The Message that could not be queued</param>
        /// <param name="innerException">The inner exception that caused the message to not be routed properly</param>
        public MessageNotQueuedException(RoutableMessage? routableMessage, Exception? innerException) : base("The Routable Message could not be queued", routableMessage, innerException) { }
    }
}
