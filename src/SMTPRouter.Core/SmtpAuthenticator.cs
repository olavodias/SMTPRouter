#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0060 // Remove unused parameter

using SmtpServer;
using SmtpServer.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter.Core
{
    /// <summary>
    /// Class to process the Authentication of the SMTPServer
    /// </summary>
    internal class SmtpAuthenticator: UserAuthenticator
    {
        public SmtpAuthenticator()
        {

        }
        public Task<bool> AuthenticateAsync(string user, string password)
        {
            // Right now, all users are allowed to relay emails. We limit that with the IP addresses.
            return Task.FromResult(true);
        }

        public override Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1822 // Mark members as static
