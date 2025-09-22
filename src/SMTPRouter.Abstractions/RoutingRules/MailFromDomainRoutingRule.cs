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

    /// <summary>
    /// Checks whether the domain specified on the <<see cref="SmtpMessage.MailFrom"/> matches the domain informed in the <see cref="Domain"/> property
    /// </summary>
    /// <remarks>The verification is case insensitive</remarks>
    /// <param name="message">The message to verify</param>
    /// <returns>A boolean to define whether the rule matches or not</returns>
    public bool Match(SmtpMessage message)
    {
        try
        {
            if (message.MailFrom is null) return false;
            if (message.MailFrom.Host is null) return false;
            if (string.IsNullOrWhiteSpace(Domain)) return false;

            return message.MailFrom.Host.Equals(Domain, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
