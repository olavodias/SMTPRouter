using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.ConfigurationSchema;

public class Connection
{
    public string? Description { get; set; }

    public string? Host { get; set; }

    public int Port { get; set; }

    public bool RequiresAuthentication { get; set; }

    public int SecureSocketOption { get; set; }

    public int ActiveConnections { get; set; }

    public int GroupingOption { get; set; }

}

