using SMTPRouter.Abstractions;
using SMTPRouter.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Core.RoutingRules;

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
    /// Checks whether the domain specified on the <see cref="SmtpMessage.MailFrom"/> matches the domain informed in the <see cref="Domain"/> property
    /// </summary>
    /// <remarks>The verification is case insensitive</remarks>
    /// <param name="message">The message to verify</param>
    /// <returns>A boolean to define whether the rule matches or not</returns>
    public bool Match(IMessage message)
    {
        try
        {
            if (message.Sender is null) return false;
            if (string.IsNullOrWhiteSpace(Domain)) return false;

            var data = message.Sender.Split('@');
            if (data is null) return false;
            if (data.Length != 2) return false;

            return data[1].Equals(Domain, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
