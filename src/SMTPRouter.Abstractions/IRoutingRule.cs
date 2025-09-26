using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Abstractions;

/// <summary>
/// Defines the interface of any routing rules
/// </summary>
/// <remarks>Routing Rules need to implement a parameterless constructor, for them to be instantiated using the Activator</remarks>
public interface IRoutingRule
{
    /// <summary>
    /// Checks whether the rule matches
    /// </summary>
    /// <param name="message">The Message to use when matching a rule</param>
    /// <returns>Whether the message matches or not</returns>
    bool Match(IMessage message);

}
