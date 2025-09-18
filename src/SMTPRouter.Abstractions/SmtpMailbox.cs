using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter;

public struct SmtpMailbox
{
    public string? User { get; set; }

    public string? Host { get; set; }

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

}
