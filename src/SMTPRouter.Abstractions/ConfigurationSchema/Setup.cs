using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.ConfigurationSchema;

public class Setup
{

    public Hosting? Hosting { get; set; }

    public Dictionary<string, Connection>? Connections { get; set; }

}
