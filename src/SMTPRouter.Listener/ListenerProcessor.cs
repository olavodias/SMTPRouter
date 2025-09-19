using Microsoft.Extensions.Logging;
using SMTPRouter.ConfigurationSchema;
using SMTPRouter.Listener;
using SmtpServer;
using SmtpServer.ComponentModel;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace SMTPRouter
{

    /// <summary>
    /// Process to listen to SMTP Messages and store them in a local folder
    /// </summary>
    public sealed class ListenerProcessor : IProcessor
    {
        private readonly ILogger<ListenerProcessor>? _logger;
        internal readonly Hosting? _hosting;
        private readonly Folders? _folders;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListenerProcessor"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="hosting">The hosting information</param>
        /// <param name="folders">The folders information</param>
        public ListenerProcessor(ILogger<ListenerProcessor>? logger, Hosting? hosting, Folders? folders)
        {
            _logger = logger;
            _hosting = hosting;
            _folders = folders;
        }

        /// <inheritdoc/>
        public Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger?.LogInformation("ListenerProcessor DoWorkAsync Start: {time}", DateTimeOffset.Now);

            // Create and Run SMTP Server
            var smtpServer = CreateSmtpServer();
            return smtpServer.StartAsync(stoppingToken);
        }
        private SmtpServer.SmtpServer CreateSmtpServer()
        {
            if (string.IsNullOrEmpty(_hosting?.Path))
                throw new InvalidOperationException("Property \"Path\" not defined");

            // Setup Paths
            CreateDirectory(Path.Combine(_hosting.Path, Folders.FILES));
            CreateDirectory(Path.Combine(_hosting.Path, Folders.FILES, Folders.FILES_LISTENER_REJECTED));
            CreateDirectory(Path.Combine(_hosting.Path, Folders.FILES, Folders.FILES_LISTENER_RECEIVED));
            CreateDirectory(Path.Combine(_hosting.Path, Folders.FILES, Folders.FILES_LISTENER_ERRORS));

            // Setup the MessageStore
            var smtpMessageStore = new SmtpMessageStore(Path.Combine(_hosting.Path, Folders.FILES));
            smtpMessageStore.MessageReceived += SmtpMessageStore_MessageReceived;
            smtpMessageStore.MessageReceivedWithErrors += SmtpMessageStore_MessageReceivedWithErrors;

            // Setup Options for the SMTP Server
            var optionsBuilder = new SmtpServerOptionsBuilder();

            // Setup Server
            if (string.IsNullOrEmpty(_hosting.Server))
                throw new InvalidOperationException("Property \"Server\" not defined");

            optionsBuilder.ServerName(_hosting.Server);

            // Setup Ports
            if (_hosting.PortsConfiguration is null)
                throw new InvalidOperationException("Dictionary \"PortsConfiguration\" not defined");

            if (_hosting.PortsConfiguration.Count == 0)
                throw new InvalidOperationException("Dictionary \"PortsConfiguration\" is defined, but empty");

            foreach (var pi in _hosting.PortsConfiguration)
            {
                optionsBuilder.Endpoint(b => b.Port(pi.Value.Number, pi.Value.IsSecure)
                                              .AllowUnsecureAuthentication(pi.Value.IsSecure)
                                              .AuthenticationRequired(_hosting.RequiresAuthentication));
            }

            // Setup Filters
            var smtpMailboxFilters = new SmtpMailboxFilter(_hosting.AcceptedIPAddresses, _hosting.RejectedIPAddresses);
            smtpMailboxFilters.MessageFiltered += SmtpMailboxFilters_MessageFiltered;

            // Setup Providers
            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new SmtpAuthenticator());
            serviceProvider.Add(smtpMessageStore);
            serviceProvider.Add(smtpMailboxFilters);

            // Initialize the SMTP Server
            var smtpServer = new SmtpServer.SmtpServer(optionsBuilder.Build(), serviceProvider);

            // Hook the events
            smtpServer.SessionCreated += Server_OnSessionCreated;
            smtpServer.SessionCompleted += Server_OnSessionCompleted;

            return smtpServer;
        }

        private void SmtpMailboxFilters_MessageFiltered(object? sender, MessageFilteredEventArgs e)
        {
            _logger?.LogWarning(LoggingEvents.UnauthorizedOrigin, "An unauthorized source is trying to relay emails. Sender is \"{from}\"; IP Address is \"{currentOrigin}\"; Size is \"{size}\";", e.MailFrom, e.OriginIPAddress, e.Size);
        }

        private static void CreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        private void SmtpMessageStore_MessageReceived(object? sender, MessageEventArgs e)
        {
            _logger?.LogInformation("Message Received Sucessfully from {sender}", e.SmtpMessage.MailFrom);
        }

        private void SmtpMessageStore_MessageReceivedWithErrors(object? sender, MessageErrorEventArgs e)
        {
            _logger?.LogError(LoggingEvents.MessageReceivedWithErrors, e.Exception, "Message Received with Errors from {sender}", e.SmtpMessage.MailFrom);
        }

        private void Server_OnSessionCreated(object? sender, SessionEventArgs e)
        {

        }
        private void Server_OnSessionCompleted(object? sender, SessionEventArgs e)
        {

        }
    }
}
