using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router.Exceptions;


public sealed class UnableToRetrieveMessageToRouteException: Exception
{

    private const string DEFAULT_MESSAGE = "The system could not get a message to route";

    public UnableToRetrieveMessageToRouteException(): this(DEFAULT_MESSAGE) { }

    public UnableToRetrieveMessageToRouteException(string message): this(message, null) { }

    public UnableToRetrieveMessageToRouteException(Exception innerException) : this(DEFAULT_MESSAGE, innerException) { }

    public UnableToRetrieveMessageToRouteException(string message, Exception? innerException): base(message, innerException) 
    {
        
    }

}
