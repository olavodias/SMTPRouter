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

    public ConfigurationSchema.Connection ConnectionInfo { get; set; }

    public RoutingConnection(string key, Folders folders, ConfigurationSchema.Connection connectionInfo)
    {
        Key = key;
        Folders = folders;
        ConnectionInfo = connectionInfo;
    }

}
