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
    internal class ListenerProcessor : IProcessor
    {
        public const string PATH_QUEUES = "Queues";

        private readonly ILogger<ListenerProcessor>? _logger;

        public ListenerProcessor()
        {
            
        }

        public ListenerProcessor(ILogger<ListenerProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The Hosting Configuration
        /// </summary>
        /// <remarks>When left null, the system will try to retrieve the configuration for a file named "listener.json"</remarks>
        public Hosting? Hosting { get; set; }

        public Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger?.LogInformation("ListenerProcessor DoWorkAsync Start: {time}", DateTimeOffset.Now);

            // Read / Validate Configuration
            if (Hosting is null)
            {
                Hosting = JsonSerializer.Deserialize<Hosting>(System.IO.File.ReadAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "listener.json"), Encoding.UTF8));

                if (Hosting is null)
                    throw new InvalidOperationException("The configuration file is not valid");
            }

            // Create and Run SMTP Server
            var smtpServer = CreateSmtpServer(Hosting);
            return smtpServer.StartAsync(stoppingToken);
        }
        private SmtpServer.SmtpServer CreateSmtpServer(Hosting hosting)
        {
            if (string.IsNullOrEmpty(hosting.Path))
                throw new InvalidOperationException("Property \"Path\" not defined");

            // Setup Paths
            CreateDirectory(Path.Combine(hosting.Path, PATH_QUEUES));
            CreateDirectory(Path.Combine(hosting.Path, PATH_QUEUES, "Outgoing"));
            CreateDirectory(Path.Combine(hosting.Path, PATH_QUEUES, "InQueue"));
            CreateDirectory(Path.Combine(hosting.Path, PATH_QUEUES, "Error"));
            CreateDirectory(Path.Combine(hosting.Path, PATH_QUEUES, "Rejected"));

            // Setup the MessageStore
            var smtpMessageStore = new SmtpMessageStore(Path.Combine(hosting.Path, PATH_QUEUES));
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
                                              .AuthenticationRequired(hosting.RequiresAuthentication));
            }

            // Setup Filters
            var smtpMailboxFilters = new SmtpMailboxFilter(hosting.AcceptedIPAddresses, hosting.RejectedIPAddresses);
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
