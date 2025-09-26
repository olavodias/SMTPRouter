using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Core
{
    /// <summary>
    /// Event Arguments for the event when a <see cref="SmtpMessage"/> has had errors during its processing
    /// </summary>
    public sealed class MessageErrorEventArgs: MessageEventArgs
    {
        /// <summary>
        /// The exception that caused the error
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Initializes a new instance of the Message Error Event Arguments
        /// </summary>
        /// <param name="smtpMessage">The <see cref="SmtpMessage"/> received by the Smtp</param>
        public MessageErrorEventArgs(SmtpMessage smtpMessage): this(smtpMessage, null) { }

        /// <summary>
        /// Initializes a new instance of the Message Error Event Arguments
        /// </summary>
        /// <param name="smtpMessage">The <see cref="SmtpMessage"/> received by the Smtp</param>
        /// <param name="exception">The exception that caused the error</param>
        public MessageErrorEventArgs(SmtpMessage smtpMessage, Exception? exception): base(smtpMessage)
        {
            SmtpMessage = smtpMessage;
            Exception = exception;
        }


    }
}
