using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.ConfigurationSchema;

/// <summary>
/// Represents an SMTP Connection to be used by the Router
/// </summary>
public class Connection
{
    /// <summary>
    /// The Description of the Connection
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The Host
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// The Port to Connect To
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Defines whether Authentication is required or not
    /// </summary>
    public bool RequiresAuthentication { get; set; }

    /// <summary>
    /// The Secure Socket Option
    /// </summary>
    public int SecureSocketOption { get; set; }

    /// <summary>
    /// Defines the Number of Active Connections
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// Defines how to group the data in the routed folders
    /// </summary>
    public int GroupingOption { get; set; }

}

