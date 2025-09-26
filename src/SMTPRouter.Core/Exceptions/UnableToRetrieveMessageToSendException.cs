using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Core.Exceptions;

/// <summary>
/// An exception thrown when the system is unable to retrieve any message to send
/// </summary>
/// <remarks>This is not necessarily a problem, as the folder could be empty</remarks>
public sealed class UnableToRetrieveMessageToSendException: Exception
{
    /// <summary>
    /// The default exception message
    /// </summary>
    private const string DEFAULT_MESSAGE = "The system could not get a message to send";


    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRetrieveMessageToSendException"/>
    /// </summary>
    public UnableToRetrieveMessageToSendException() : this(DEFAULT_MESSAGE) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRetrieveMessageToSendException"/>
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    public UnableToRetrieveMessageToSendException(string message) : this(message, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRetrieveMessageToSendException"/>
    /// </summary>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public UnableToRetrieveMessageToSendException(Exception innerException) : this(DEFAULT_MESSAGE, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnableToRetrieveMessageToSendException"/>
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public UnableToRetrieveMessageToSendException(string message, Exception? innerException) : base(message, innerException)
    {

    }


}
