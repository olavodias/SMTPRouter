using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter.Core;

/// <summary>
/// Defines an Smtp Mailbox
/// </summary>
public class SmtpMailbox: IEquatable<SmtpMailbox>
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
    /// <param name="address">The email address</param>
    public SmtpMailbox(string address)
    {
        var data = address.Split('@');
        if (data.Length != 2)
            throw new FormatException("The address is not formatted like an email");

        //TODO: Perhaps, add a Regex to validate the mail from

        User = data[0];
        Host = data[1];
    }

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
    public override string ToString()
    {
        return $"{User}@{Host}";
    }

    

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not SmtpMailbox other) return false;
        return User == other.User && Host == other.Host;
    }

    /// <inheritdoc/>
    public bool Equals(SmtpMailbox? other)
    {
        if (other is null) return false;
        return User == other.User && Host == other.Host;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

}
