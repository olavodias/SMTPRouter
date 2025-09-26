using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Core.ConfigurationSchema;

/// <summary>
/// Defines the Message Purging Configuration
/// </summary>
public sealed class MessagePurge
{
    /// <summary>
    /// Defines whether the purge of messages is enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The number of days a message should remain on the server
    /// </summary>
    public int DaysToKeep { get; set; }

    //TODO: maybe implement a policy for processed messages, error, rejected, etc.
}
