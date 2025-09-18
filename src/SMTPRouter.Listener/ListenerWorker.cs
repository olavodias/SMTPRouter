#pragma warning disable IDE0270 // Use coalesce expression

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Tls;
using SMTPRouter.ConfigurationSchema;
using SmtpServer;
using SmtpServer.ComponentModel;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter.Listener
{
    /// <summary>
    /// The Worker that will listen to SMTP Messages and save them
    /// </summary>
    public class ListenerWorker : BackgroundService
    {
        private readonly ILogger<ListenerWorker> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListenerWorker"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        public ListenerWorker(ILogger<ListenerWorker> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker Executing Async: {time}", DateTimeOffset.Now);

            try
            {
                // Read / Validate Configuration
                var hosting = JsonSerializer.Deserialize<Hosting>(System.IO.File.ReadAllText("listener.json", Encoding.UTF8));

                if (hosting is null)
                    throw new InvalidOperationException("The configuration file is not valid");

                // Initialize SMTP Server
                var smtpServer = CreateSmtpServer(hosting);

                //TODO: CONTINUE FROM HERE

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        
                    }
                    await Task.Delay(1000, stoppingToken);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private SmtpServer.SmtpServer CreateSmtpServer(Hosting hosting)
        {
            if (string.IsNullOrEmpty(hosting.Path))
                throw new InvalidOperationException("Property \"Path\" not defined");

            // Setup the MessageStore
            var smtpMessageStore = new SmtpMessageStore(hosting.Path);
            smtpMessageStore.MessageReceived += SmtpMessageStore_MessageReceived;
            smtpMessageStore.MessageReceivedWithErrors += SmtpMessageStore_MessageReceivedWithErrors;

            // Setup Options for the SMTP Server
            var optionsBuilder = new SmtpServerOptionsBuilder();

            // Setup Server
            if (string.IsNullOrEmpty(hosting.Server))
                throw new InvalidOperationException("Property \"Server\" not defined");

            optionsBuilder.ServerName(hosting.Server);

            // Setup Ports
            if (hosting.PortsConfiguration is null)
                throw new InvalidOperationException("Dictionary \"PortsConfiguration\" not defined");

            if (hosting.PortsConfiguration.Count == 0)
                throw new InvalidOperationException("Dictionary \"PortsConfiguration\" is defined, but empty");

            foreach (var pi in hosting.PortsConfiguration)
            {
                optionsBuilder.Endpoint(b => b.Port(pi.Value.Number, pi.Value.IsSecure)
                                              .AllowUnsecureAuthentication(pi.Value.IsSecure)
                                              .AuthenticationRequired(hosting.RequiresAuthentication));            }

            // Setup Providers
            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(smtpMessageStore);

            //TODO: Allow the use of a custom authenticator class instead of using this built in
            serviceProvider.Add(new SmtpAuthenticator());

            //TODO: Add filters to accept or reject messages from certain IP addresses

            // Initialize the SMTP Server
            var smtpServer = new SmtpServer.SmtpServer(optionsBuilder.Build(), serviceProvider);

            // Hook the events
            smtpServer.SessionCreated += Server_OnSessionCreated;
            smtpServer.SessionCompleted += Server_OnSessionCompleted;

            return smtpServer;
        }

        private void SmtpMessageStore_MessageReceived(object? sender, MessageEventArgs e)
        {

        }

        private void SmtpMessageStore_MessageReceivedWithErrors(object? sender, MessageErrorEventArgs e)
        {

        }

        private void Server_OnSessionCreated(object? sender, SessionEventArgs e)
        {

        }
        private void Server_OnSessionCompleted(object? sender, SessionEventArgs e)
        {

        }


    }
}

#pragma warning restore IDE0270 // Use coalesce expression
