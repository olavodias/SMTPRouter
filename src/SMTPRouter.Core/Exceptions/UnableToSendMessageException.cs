using SMTPRouter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Core.Exceptions;

/// <summary>
/// An exception thrown when the system could not send a message
/// </summary>
public sealed class UnableToSendMessageException: Exception
{
    /// <summary>
    /// The default error message
    /// </summary>
    private const string DEFAULT_MESSAGE = "The system could not send the message";

    /// <summary>
    /// Reference to the message that could not be routed
    /// </summary>
    public SmtpMessage SmtpMessage { get; }

    /// <summary>
    /// The name of the file that could not be sent
    /// </summary>
    public string? FileToSend { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToSendMessageException"/> class
    /// </summary>
    /// <param name="smtpMessage">Reference to the message that could not be sent</param>
    /// <param name="fileToSend">The name of the file that could not be sent</param>
    public UnableToSendMessageException(SmtpMessage smtpMessage, string? fileToSend) : this(smtpMessage, fileToSend, DEFAULT_MESSAGE) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToSendMessageException"/> class
    /// </summary>
    /// <param name="smtpMessage">Reference to the message that could not be sent</param>
    /// <param name="fileToSend">The name of the file that could not be sent</param>
    /// <param name="message">The error message that explains the reason for the exception</param>
    public UnableToSendMessageException(SmtpMessage smtpMessage, string? fileToSend, string message) : this(smtpMessage, fileToSend, message, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToSendMessageException"/> class
    /// </summary>
    /// <param name="smtpMessage">Reference to the message that could not be sent</param>
    /// <param name="fileToSend">The name of the file that could not be sent</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public UnableToSendMessageException(SmtpMessage smtpMessage, string? fileToSend, Exception? innerException) : this(smtpMessage, fileToSend, DEFAULT_MESSAGE, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToSendMessageException"/> class
    /// </summary>
    /// <param name="smtpMessage">Reference to the message that could not be sent</param>
    /// <param name="fileToSend">The name of the file that could not be sent</param>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public UnableToSendMessageException(SmtpMessage smtpMessage, string? fileToSend, string message, Exception? innerException) : base(message, innerException)
    {
        SmtpMessage = smtpMessage;
        FileToSend = fileToSend;
    }


}
