using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter;

/// <summary>
/// Represents the Arguments for an event triggered when a message is filtered
/// </summary>
public sealed class MessageFilteredEventArgs: EventArgs
{
    /// <summary>
    /// The Sender of the Message
    /// </summary>
    public string MailFrom { get; }

    /// <summary>
    /// The Source IP Address
    /// </summary>
    public string OriginIPAddress { get; }

    /// <summary>
    /// The Size of the Message
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageFilteredEventArgs"/> Event Arguments
    /// </summary>
    /// <param name="mailFrom">The Sender of the Message</param>
    /// <param name="originIpAddress">The Source IP Address</param>
    /// <param name="size">The size of the message</param>
    public MessageFilteredEventArgs(string mailFrom, string originIpAddress, int size)
    {
        MailFrom = mailFrom;
        OriginIPAddress = originIpAddress;
        Size = size;
    }


}
