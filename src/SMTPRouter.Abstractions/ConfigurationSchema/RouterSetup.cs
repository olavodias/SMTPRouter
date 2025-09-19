using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.ConfigurationSchema;

/// <summary>
/// The class representing the Router configuration schema
/// </summary>
public sealed class RouterSetup
{
    /// <summary>
    /// The path where the listener and router store the files
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// The SMTP Connections
    /// </summary>
    public Dictionary<string, Connection>? Connections { get; set; }

    /// <summary>
    /// The Rules to Route Emails
    /// </summary>
    public Dictionary<string, Rule>? Rules { get; set; }

}
