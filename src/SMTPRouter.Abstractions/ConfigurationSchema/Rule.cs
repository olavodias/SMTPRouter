using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.ConfigurationSchema;

/// <summary>
/// Defines the rule and its characteristics
/// </summary>
public sealed class Rule
{
    /// <summary>
    /// The Type Name used by the Rule
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// A dictionary of parameters for the Rule
    /// </summary>
    /// <remarks>The parameter name must match a property existing in the type defined at the <see cref="Type"/> property</remarks>
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// The key of the SMTP Connection to relay the message to
    /// </summary>
    public string? ConnectionKey { get; set; }


}
