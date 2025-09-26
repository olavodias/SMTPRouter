using SMTPRouter.Core.ConfigurationSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Core;

/// <summary>
/// Defines a Routing Connection
/// </summary>
public sealed class RoutingConnection
{
    /// <summary>
    /// The key of the connection
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The folder structure object
    /// </summary>
    public Folders Folders { get; set; }

    /// <summary>
    /// The maximum number of attempts the system will retry to send a message thru this connection
    /// </summary>
    public int MaximumRetryAttempts { get; set; }

    /// <summary>
    /// Configuration information regarding the connection
    /// </summary>
    public Connection ConnectionInfo { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoutingConnection"/> class
    /// </summary>
    /// <param name="key">The key of the connection</param>
    /// <param name="folders">The folder structure object</param>
    /// <param name="connectionInfo">The configuration information regarding the connection</param>
    public RoutingConnection(string key, Folders folders, Connection connectionInfo)
    {
        Key = key;
        Folders = folders;
        ConnectionInfo = connectionInfo;

        if (connectionInfo.MaximumRetryAttempts < 0)
            MaximumRetryAttempts = 0;
        else if (connectionInfo.MaximumRetryAttempts > 5)
            MaximumRetryAttempts = 5;
        else
            MaximumRetryAttempts = connectionInfo.MaximumRetryAttempts;
    }
}
