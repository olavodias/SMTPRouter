using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router.Exceptions;

public sealed class UnableToSendMessageException: Exception
{
    private const string DEFAULT_MESSAGE = "The system could not send the message";

    public SmtpMessage SmtpMessage { get; }

    public string? FileToSend { get; }

    public UnableToSendMessageException(SmtpMessage smtpMessage, string? fileToSend) : this(smtpMessage, fileToSend, DEFAULT_MESSAGE) { }

    public UnableToSendMessageException(SmtpMessage smtpMessage, string? fileToSend, string message) : this(smtpMessage, fileToSend, message, null) { }

    public UnableToSendMessageException(SmtpMessage smtpMessage, string? fileToSend, Exception? innerException) : this(smtpMessage, fileToSend, DEFAULT_MESSAGE, innerException) { }

    public UnableToSendMessageException(SmtpMessage smtpMessage, string? fileToSend, string message, Exception? innerException) : base(message, innerException)
    {
        SmtpMessage = smtpMessage;
        FileToSend = fileToSend;
    }


}
