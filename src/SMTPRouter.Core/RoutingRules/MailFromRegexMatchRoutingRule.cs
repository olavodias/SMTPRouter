using SMTPRouter.Abstractions;
using SMTPRouter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SMTPRouter.Core.RoutingRules;

/// <summary>
/// Represents a <see cref="IRoutingRule"/> which checks whether if the mail from matches a regular expression
/// </summary>
public sealed class MailFromRegexMatchRoutingRule : IRoutingRule
{
    /// <summary>
    /// The Expression to Match
    /// </summary>
    public string? MatchExpression { get; set; } = string.Empty;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MailFromRegexMatchRoutingRule"/> class
    /// </summary>
    public MailFromRegexMatchRoutingRule()
    {
        
    }

    /// <inheritdoc/>
    public bool Match(IMessage message)
    {
        try
        {
            if (message.Sender is null) return false;
            if (string.IsNullOrWhiteSpace(MatchExpression)) return false;

            return Regex.Match(message.Sender, MatchExpression).Success;
        }
        catch
        {
            return false;
        }
    }
}
