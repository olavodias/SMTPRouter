using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter;

/// <summary>
/// Defines the logging Event IDs for better filtering
/// </summary>
public static class LoggingEvents
{

    /// <summary>
    /// The source of the message is not authorized to relay emails thru the SMTP
    /// </summary>
    public static EventId UnauthorizedOrigin = new(1000, nameof(UnauthorizedOrigin));

    /// <summary>
    /// The message was received but there were errors during the save process
    /// </summary>
    public static EventId MessageReceivedWithErrors = new(1001, name: nameof(MessageReceivedWithErrors));

}
