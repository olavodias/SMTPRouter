using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer;
using SmtpServer.ComponentModel;

namespace SMTPRouter
{
    /// <summary>
    /// A class representing an SMTP Listener
    /// </summary>
    /// <remarks>The Listener will listen to Smtp Commands and fire events when messages are received</remarks>
    /// <example>
    /// A <see cref="Listener"/> can be created using the code below:
    /// <code>
    /// // Create the Listener
    /// var listener = new SMTPRouter.Listener()
    /// {
    ///     ServerName = "localhost",
    ///     Ports = new int[] { 25, 587 },
    ///     RequiresAuthentication = false,
    ///     UseSSL = false
    /// };
    /// 
    /// // Hook into events
    /// listener.ListeningStarted += Server_ListeningStarted;
    /// listener.SessionCreated += Server_SessionCreated;
    /// listener.SessionCommandExecuting += Server_SessionCommandExecuting;
    /// listener.SessionCompleted += Server_SessionCompleted;
    /// listener.MessageReceived += Server_MessageReceived;
    /// </code>
    /// </example>
    public sealed class Listener
    {
        //TODO: Implement a Listener Builder Instead

        /// <summary>
        /// The Default Ports to Use
        /// </summary>
        readonly static List<PortInfo> DefaultPorts = new()
        {
            new PortInfo() { PortNumber = 25, IsSecure = false },
            new PortInfo() { PortNumber = 578, IsSecure = true }
        };

        /// <summary>
        /// Reference to the SmtpServer
        /// </summary>
        internal SmtpServer.SmtpServer? InternalSmtpServer { get; private set; }
        /// <summary>
        /// Defines whether the Listener is active or not
        /// </summary>
        public bool IsListening { get; private set; }
        /// <summary>
        /// Name of the Server where the services will run
        /// </summary>
        public string? ServerName { get; set; }
        /// <summary>
        /// Ports where the SMTP Service will be available
        /// </summary>
        public List<PortInfo> Ports { get; set; } = new List<PortInfo>();
        /// <summary>
        /// Defines whether the SMTP Requires authentication
        /// </summary>
        public bool RequiresAuthentication { get; set; }

        /// <summary>
        /// Event triggered when the Listener started to listen to smtp messages
        /// </summary>
        public event EventHandler<EventArgs>? ListeningStarted;

        /// <summary>
        /// Event triggered when a message is received
        /// </summary>
        public event EventHandler<MessageEventArgs>? MessageReceived;

        /// <summary>
        /// Event triggered when a message is received but not processed
        /// </summary>
        public event EventHandler<MessageErrorEventArgs>? MessageReceivedWithErrors;

        /// <summary>
        /// Event triggered when an SMTP Session is created
        /// </summary>
        public event EventHandler<SessionEventArgs>? SessionCreated;

        /// <summary>
        /// Event triggered when an SMTP Session is completed
        /// </summary>
        public event EventHandler<SessionEventArgs>? SessionCompleted;

        /// <summary>
        /// Event triggered when an SMTP Command is executing
        /// </summary>
        public event EventHandler<SmtpCommandEventArgs>? SessionCommandExecuting;

        /// <summary>
        /// Initializes a new instance of a Smtp Listener
        /// </summary>
        public Listener(): this("", Listener.DefaultPorts) { }

        /// <summary>
        /// Initializes a new instance of a Smtp Listener
        /// </summary>
        /// <param name="serverName">The Server Name (usually localhost)</param>
        /// <param name="ports">Ports where the service will be available</param>
        public Listener(string serverName, List<PortInfo> ports): this(serverName, ports, false) { }

        /// <summary>
        /// Initializes a new instance of a Smtp Listener
        /// </summary>
        /// <param name="serverName">The Server Name (usually localhost)</param>
        /// <param name="ports">Ports where the service will be available</param>
        /// <param name="requiresAuthentication">A flag to define whether authentication is required for this smtp server</param>
        public Listener(string serverName, List<PortInfo> ports, bool requiresAuthentication)
        {
            this.ServerName = serverName;
            this.Ports = ports;
            this.RequiresAuthentication = requiresAuthentication;

            IsListening = false;
        }

        /// <summary>
        /// Initializes a new instance of the Listener
        /// </summary>
        /// <param name="cancellationToken">The Cancellation Token to stop a transaction</param>
        /// <returns>An awaitable <see cref="Task"/> with the listener to Smtp Messages</returns>
        /// <exception cref="ArgumentException">Throw when there are no <see cref="Ports"/> defined</exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Setup the MessageStore
            var smtpMessageStore = new SmtpMessageStore();
            smtpMessageStore.MessageReceived += SmtpMessageStore_MessageReceived;
            smtpMessageStore.MessageReceivedWithErrors += SmtpMessageStore_MessageReceivedWithErrors;

            // Setup Options for the SMTP Server
            var optionsBuilder = new SmtpServerOptionsBuilder();

            // Setup Server
            if (string.IsNullOrEmpty(ServerName))
                throw new ArgumentException("The Server Name is not defined");

            optionsBuilder.ServerName(this.ServerName);

            // Setup Ports
            if (this.Ports.Count == 0)
                throw new ArgumentException("At least one port must be setup");

            foreach (var pi in this.Ports)
            {
                optionsBuilder.Endpoint(b => b.Port(pi.PortNumber, pi.IsSecure)
                                              .AllowUnsecureAuthentication(pi.IsSecure)
                                              .AuthenticationRequired(this.RequiresAuthentication));
            }

            // Setup Providers
            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(smtpMessageStore);

            //TODO: Allow the use of a custom authenticator class instead of using this built in
            serviceProvider.Add(new SmtpAuthenticator());

            //TODO: Add filters to accept or reject messages from certain IP addresses

            // Initialize the SMTP Server
            InternalSmtpServer = new SmtpServer.SmtpServer(optionsBuilder.Build(), serviceProvider);

            // Hook the events
            InternalSmtpServer.SessionCreated += Server_OnSessionCreated;
            InternalSmtpServer.SessionCompleted += Server_OnSessionCompleted;

            // Sets the Listening to on and kick event
            IsListening = true;
            ListeningStarted?.Invoke(this, EventArgs.Empty);

            // Starts the SMTP Server
            await InternalSmtpServer.StartAsync(cancellationToken);
        }

        private void SmtpMessageStore_MessageReceived(object sender, MessageEventArgs e)
        {
            // Trigger the MessageReceived event for the Listener
            MessageReceived?.Invoke(sender, e);
        }

        private void SmtpMessageStore_MessageReceivedWithErrors(object sender, MessageErrorEventArgs e)
        {
            // Trigger the MessageReceivedWithErrors for the Listener
            MessageReceivedWithErrors?.Invoke(sender, e);
        }

        private void Server_OnSessionCreated(object sender, SessionEventArgs e)
        {
            // Hook event to the Session
            e.Context.CommandExecuting += Context_CommandExecuting;

            // Trigger the Session Created event for the Listener
            SessionCreated?.Invoke(sender, e);
        }

        private void Context_CommandExecuting(object sender, SmtpCommandEventArgs e)
        {
            // Trigger the Session Command Executing event for the Listener
            SessionCommandExecuting?.Invoke(sender, e);
        }

        private void Server_OnSessionCompleted(object sender, SessionEventArgs e)
        {
            // Trigger the Session Created event for the Listener
            SessionCompleted?.Invoke(sender, e);
        }

    }

    /// <summary>
    /// Information regarding a port to be used
    /// </summary>
    public struct PortInfo
    {
        /// <summary>
        /// The port number
        /// </summary>
        public int PortNumber;
        /// <summary>
        /// Defines whether the port is secure or not
        /// </summary>
        public bool IsSecure;
    }

}
