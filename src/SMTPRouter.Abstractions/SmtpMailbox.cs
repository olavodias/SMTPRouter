using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter;

/// <summary>
/// Defines an Smtp Mailbox
/// </summary>
public struct SmtpMailbox
{
    /// <summary>
    /// The User
    /// </summary>
    public string? User { get; set; }

    /// <summary>
    /// The Host
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpMailbox"/> class
    /// </summary>
    /// <param name="user">The User</param>
    /// <param name="host">The Host</param>
    public SmtpMailbox(string user, string host)
    {
        User = user;
        Host = host;
    }

    /// <inheritdoc/>
    public readonly override string ToString()
    {
        return $"{User}@{Host}";
    }

}
