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
    /// The maximum number of attemps the system will try to send an email
    /// </summary>
    public int MaximumRetryAttempts { get; set; }

    /// <summary>
    /// Defines how to group the data in the routed folders
    /// </summary>
    public GroupingOptions GroupingOption { get; set; }

}

/// <summary>
///  Grouping options for the files on the Sent folder
/// </summary>
public enum GroupingOptions: byte
{
    /// <summary>
    /// All files will be saved on the root folder
    /// </summary>
    NoGrouping = 0,
    /// <summary>
    /// All files will be saved on a folder per day
    /// </summary>
    GroupByDate = 1,
    /// <summary>
    /// All files will be saved on a folder per day and hour
    /// </summary>
    GroupByDateAndHour = 2
}
