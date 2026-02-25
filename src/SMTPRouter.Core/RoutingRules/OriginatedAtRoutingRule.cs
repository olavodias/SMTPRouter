using SMTPRouter.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Core.RoutingRules;

/// <summary>
/// Represents a <see cref="IRoutingRule"/> which checks whether the IP address on the incoming message mathes a specific IP address
/// </summary>
public sealed class OriginatedAtRoutingRule: IRoutingRule
{
    /// <summary>
    /// The IP Address to match
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OriginatedAtRoutingRule"/> class
    /// </summary>
    public OriginatedAtRoutingRule()
    {
        
    }

    /// <summary>
    /// Checks whether the origin specified at <see cref="SmtpMessage.OriginIPAddress"/> matches the <see cref="IPAddress"/> property
    /// </summary>
    /// <param name="message">The message to verify</param>
    /// <returns>A boolean to define whether the rule matches or not</returns>
    public bool Match(IMessage message)
    {
        try
        {
            if (message.Sender is null) return false;
            if (string.IsNullOrWhiteSpace(IPAddress)) return false;
            if (string.IsNullOrWhiteSpace(message.OriginIPAddress)) return false;

            return string.Equals(message.OriginIPAddress, IPAddress, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

}
