using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.RoutingRules;

/// <summary>
/// Represents a <see cref="IRoutingRule"/> which always return as a positive match. Use it when you just need to intercept emails before re-routing them to the destination.
/// </summary>
public sealed class RelayRoutingRule : IRoutingRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelayRoutingRule"/> class
    /// </summary>
    public RelayRoutingRule()
    {
        
    }

    /// <inheritdoc/>
    public bool Match(SmtpMessage message)
    {
        return true;
    }
}
