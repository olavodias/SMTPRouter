using SMTPRouter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Core.Exceptions;

/// <summary>
/// An exception thrown when the system could not route a message
/// </summary>
public sealed class UnableToRouteMessageException: Exception
{
    /// <summary>
    /// The default error message
    /// </summary>
    private const string DEFAULT_MESSAGE = "The system could not route the message";

    /// <summary>
    /// Reference to the message that could not be routed
    /// </summary>
    public SmtpMessage SmtpMessage { get; }

    /// <summary>
    /// The name of the file that could not be routed
    /// </summary>
    public string? FileToRoute { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRouteMessageException"/> class
    /// </summary>
    /// <param name="smtpMessage">Reference to the message that could not be routed</param>
    /// <param name="fileToRoute">The name of the file that could not be routed</param>
    public UnableToRouteMessageException(SmtpMessage smtpMessage, string? fileToRoute): this(smtpMessage, fileToRoute, DEFAULT_MESSAGE) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRouteMessageException"/> class
    /// </summary>
    /// <param name="smtpMessage">Reference to the message that could not be routed</param>
    /// <param name="fileToRoute">The name of the file that could not be routed</param>
    /// <param name="message">The error message that explains the reason for the exception</param>
    public UnableToRouteMessageException(SmtpMessage smtpMessage, string? fileToRoute, string message): this(smtpMessage, fileToRoute, message, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRouteMessageException"/> class
    /// </summary>
    /// <param name="smtpMessage">Reference to the message that could not be routed</param>
    /// <param name="fileToRoute">The name of the file that could not be routed</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public UnableToRouteMessageException(SmtpMessage smtpMessage, string? fileToRoute, Exception? innerException) : this(smtpMessage, fileToRoute, DEFAULT_MESSAGE, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRouteMessageException"/> class
    /// </summary>
    /// <param name="smtpMessage">Reference to the message that could not be routed</param>
    /// <param name="fileToRoute">The name of the file that could not be routed</param>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public UnableToRouteMessageException(SmtpMessage smtpMessage, string? fileToRoute, string message, Exception? innerException): base(message, innerException)
    {
        SmtpMessage = smtpMessage;
        FileToRoute = fileToRoute;
    }
}
