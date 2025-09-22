using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router.Exceptions;

public sealed class UnableToRetrieveMessageToSendException: Exception
{
    private const string DEFAULT_MESSAGE = "The system could not get a message to send";

    public UnableToRetrieveMessageToSendException() : this(DEFAULT_MESSAGE) { }

    public UnableToRetrieveMessageToSendException(string message) : this(message, null) { }

    public UnableToRetrieveMessageToSendException(Exception innerException) : this(DEFAULT_MESSAGE, innerException) { }

    public UnableToRetrieveMessageToSendException(string message, Exception? innerException) : base(message, innerException)
    {

    }


}
