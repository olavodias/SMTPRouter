using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.RoutingRules;

/// <summary>
/// Defines the interface of any routing rules
/// </summary>
/// <remarks>Routing Rules need to implement a parameterless constructor, for them to be instantiated using the Activator</remarks>
public interface IRoutingRule
{
    /// <summary>
    /// Checks whether the rule matches
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    bool Match(SmtpMessage message);

}
