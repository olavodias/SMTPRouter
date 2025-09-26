using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Core.Exceptions;


/// <summary>
/// Defines an exception thrown when the system is unable to locate a message to be routed
/// </summary>
/// <remarks>This by itself is not necessarily a problem, as the folder can be empty</remarks>
public sealed class UnableToRetrieveMessageToRouteException: Exception
{
    /// <summary>
    /// The default exception message
    /// </summary>
    private const string DEFAULT_MESSAGE = "The system could not get a message to route";

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRetrieveMessageToRouteException"/>
    /// </summary>
    public UnableToRetrieveMessageToRouteException(): this(DEFAULT_MESSAGE) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRetrieveMessageToRouteException"/>
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    public UnableToRetrieveMessageToRouteException(string message): this(message, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRetrieveMessageToRouteException"/>
    /// </summary>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public UnableToRetrieveMessageToRouteException(Exception innerException) : this(DEFAULT_MESSAGE, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRetrieveMessageToRouteException"/>
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public UnableToRetrieveMessageToRouteException(string message, Exception? innerException): base(message, innerException) 
    {
        
    }

}
