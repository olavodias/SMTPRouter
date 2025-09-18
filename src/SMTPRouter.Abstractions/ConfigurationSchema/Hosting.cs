using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.ConfigurationSchema;

public sealed class Hosting
{

    public string? Server { get; set; }

    public string? Path { get; set; }

    public string? MessageLifespan { get; set; }

    public int MessagePurgeLifespan { get; set; }

    public bool RequiresAuthentication { get; set; }

    public Dictionary<string, PortConfiguration>? PortsConfiguration { get; set; }

    public string[]? AcceptedIPAddresses { get; set; }

    public string[]? RejectedIPAddresses { get; set; }

}
