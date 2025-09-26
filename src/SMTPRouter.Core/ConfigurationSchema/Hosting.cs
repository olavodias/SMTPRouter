using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Core.ConfigurationSchema;

/// <summary>
/// Represents the Hosting Configuration Section
/// </summary>
public sealed class Hosting
{

    /// <summary>
    /// The Server Name
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// The Root Path of the Hosting
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// The lifespan of a message
    /// </summary>
    /// <remarks>When a message exceeds the Lifespan, it is considered expired</remarks>
    public string? MessageLifespan { get; set; }

    /// <summary>
    /// Defines whether the SMTP requires authentication or not
    /// </summary>
    public bool RequiresAuthentication { get; set; }

    /// <summary>
    /// The configuration of endpoints
    /// </summary>
    public Dictionary<string, PortConfiguration>? PortsConfiguration { get; set; }

    /// <summary>
    /// An array containing the IP addresses that can relay messages thru the SMTP
    /// </summary>
    /// <remarks>This parameter is combined with the <see cref="RejectedIPAddresses"/> to define which addresses can be used for relaying messages</remarks>
    public string[]? AcceptedIPAddresses { get; set; }

    /// <summary>
    /// An array containing the IP addresses that are prohibted from relaying messages thru the SMTP
    /// </summary>
    /// <remarks>This parameter is combined with the <see cref="AcceptedIPAddresses"/> to define which addresses can beused for relaying messages</remarks>
    public string[]? RejectedIPAddresses { get; set; }

    /// <summary>
    /// The Message Purge Configuration
    /// </summary>
    public MessagePurge? MessagePurge { get; set; }

}
