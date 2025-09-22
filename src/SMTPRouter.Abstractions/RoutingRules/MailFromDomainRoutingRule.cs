using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.RoutingRules;

/// <summary>
/// Represents a <see cref="IRoutingRule"/> which checks whether the domain on the mail sender matches a specified domain
/// </summary>
public sealed class MailFromDomainRoutingRule : IRoutingRule
{
    /// <summary>
    /// The Domain to match
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MailFromDomainRoutingRule"/> class
    /// </summary>
    public MailFromDomainRoutingRule()
    {
        
    }

    /// <inheritdoc/>
    public bool Match(SmtpMessage message)
    {
        try
        {
            if (message.MailFrom is null) return false;
            if (message.MailFrom.Host is null) return false;
            if (string.IsNullOrWhiteSpace(Domain)) return false;

            return message.MailFrom.Host.Equals(Domain);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
