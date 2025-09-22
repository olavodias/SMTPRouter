using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router.Exceptions;

public sealed class UnableToRouteMessageException: Exception
{
    private const string DEFAULT_MESSAGE = "The system could not route the message";

    public SmtpMessage SmtpMessage { get; }

    public string? FileToRoute { get; }

    public UnableToRouteMessageException(SmtpMessage smtpMessage, string? fileToRoute): this(smtpMessage, fileToRoute, DEFAULT_MESSAGE) { }
    
    public UnableToRouteMessageException(SmtpMessage smtpMessage, string? fileToRoute, string message): this(smtpMessage, fileToRoute, message, null) { }

    public UnableToRouteMessageException(SmtpMessage smtpMessage, string? fileToRoute, Exception? innerException) : this(smtpMessage, fileToRoute, DEFAULT_MESSAGE, innerException) { }

    public UnableToRouteMessageException(SmtpMessage smtpMessage, string? fileToRoute, string message, Exception? innerException): base(message, innerException)
    {
        SmtpMessage = smtpMessage;
        FileToRoute = fileToRoute;
    }


}
