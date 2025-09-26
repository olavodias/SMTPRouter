using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Core.ConfigurationSchema;

/// <summary>
/// Defines the Port Configuration Information
/// </summary>
public struct PortConfiguration
{
    /// <summary>
    /// The Port Number
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Defines whether to use SSL or not
    /// </summary>
    public bool IsSecure { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PortConfiguration"/> class
    /// </summary>
    /// <param name="number">The Port Number</param>
    /// <param name="isSecure">Defines whether to use SSL oir not</param>
    public PortConfiguration(int number, bool isSecure)
    {
        Number = number;
        IsSecure = isSecure;
    }
}
