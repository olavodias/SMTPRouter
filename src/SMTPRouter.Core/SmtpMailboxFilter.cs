using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Net;
using SmtpServer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter.Core;

internal sealed class SmtpMailboxFilter : IMailboxFilter
{
    /// <summary>
    /// Event triggered when a Message is Filtered
    /// </summary>
    public event EventHandler<MessageFilteredEventArgs>? MessageFiltered;

    public List<string> AcceptedIPAddresses { get; private set; }
    public List<string> RejectedIPAddresses { get; private set; }

    public SmtpMailboxFilter(string[]? acceptedIPAddresses, string[]? rejectedIPAddresses)
    {
        AcceptedIPAddresses = acceptedIPAddresses is null ? new List<string>() : acceptedIPAddresses.ToList();
        RejectedIPAddresses = rejectedIPAddresses is null ? new List<string>() : rejectedIPAddresses.ToList();
    }

    /// <inheritdoc/>
    public Task<bool> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size, CancellationToken cancellationToken)
    {
        var endpoint = (IPEndPoint)context.Properties[EndpointListener.RemoteEndPointKey];
        var currentFrom = from?.AsAddress() ?? string.Empty;
        var currentOrigin = endpoint.Address.ToString();
        
        if (AcceptedIPAddresses.Count > 0)
        {
            if (!AcceptedIPAddresses.Contains(currentOrigin))
            {
                MessageFiltered?.Invoke(this, new MessageFilteredEventArgs(currentFrom, currentOrigin, size));
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        if (RejectedIPAddresses.Count > 0)
        {
            if (RejectedIPAddresses.Contains(endpoint.Address.ToString()))
            {
                MessageFiltered?.Invoke(this, new MessageFilteredEventArgs(currentFrom, currentOrigin, size));
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<bool> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from, CancellationToken cancellationToken)
    {
        // All deliveries can be made
        return Task.FromResult(true);
    }
}
