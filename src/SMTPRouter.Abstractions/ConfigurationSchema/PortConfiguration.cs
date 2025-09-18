using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.ConfigurationSchema;

public sealed class PortConfiguration
{

    public int Number { get; set; }

    public bool IsSecure { get; set; }

}
