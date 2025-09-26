using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Abstractions;

/// <summary>
/// Defines a Message
/// </summary>
public interface IMessage
{
    /// <summary>
    /// The IP Address Sending the Message
    /// </summary>
    string? OriginIPAddress { get; set; }

    /// <summary>
    /// The sender of the Message
    /// </summary>
    string? Sender { get; set; }

    /// <summary>
    /// The Contents of the Message
    /// </summary>
    string? Contents { get; set; }

}
