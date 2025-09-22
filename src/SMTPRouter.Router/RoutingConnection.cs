using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router;

public sealed class RoutingConnection
{
    public string Key { get; set; }

    public Folders Folders { get; set; }

    public int MaximumRetryAttempts { get; set; }

    public ConfigurationSchema.Connection ConnectionInfo { get; set; }

    public RoutingConnection(string key, Folders folders, ConfigurationSchema.Connection connectionInfo)
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
