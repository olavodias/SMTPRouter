using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Core;

/// <summary>
/// Defines the logging Event IDs for better filtering
/// </summary>
public static class LoggingEvents
{

    // **********************************************************************
    // General Events
    // **********************************************************************

    /// <summary>
    /// The source of the message is not authorized to relay emails thru the SMTP
    /// </summary>
    public static EventId UnauthorizedOrigin = new(1000, nameof(UnauthorizedOrigin));

    /// <summary>
    /// The message was received but there were errors during the save process
    /// </summary>
    public static EventId MessageReceivedWithErrors = new(1001, nameof(MessageReceivedWithErrors));

    /// <summary>
    /// There was a IO error. Either a file could not be located, or moved, or deleted
    /// </summary>
    public static EventId FileIOError = new(1002, nameof(FileIOError));

    // **********************************************************************
    // Routing Events
    // **********************************************************************

    /// <summary>
    /// The system failed when attempting to retrieve messages to route
    /// </summary>
    /// <remarks>This is thrown if something failed in the process, but there were files to be processed</remarks>
    public static EventId UnableToRetrieveMessagesToRoute = new(2000, nameof(UnableToRetrieveMessagesToRoute));

    /// <summary>
    /// There were errors during the routing process
    /// </summary>
    public static EventId RoutingErrors = new(2001, nameof(RoutingErrors));

    // **********************************************************************
    // Connection Events
    // **********************************************************************

    /// <summary>
    /// The system failed when attempting to retrieve messages to send
    /// </summary>
    /// <remarks>This is thrown if something failed in the process, but there were files to be processed</remarks>
    public static EventId UnableToRetrieveMessagesToSend = new(2500, nameof(UnableToRetrieveMessagesToRoute));

    /// <summary>
    /// The system failed when attempting to send a message thru an active connection
    /// </summary>
    public static EventId UnableToSendMessage = new(2501, nameof(UnableToSendMessage));


}
