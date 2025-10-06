using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Net;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter.Core
{
    /// <summary>
    /// The Message Store that handles the incoming SMTP messages
    /// </summary>
    internal sealed class SmtpMessageStore: MessageStore
    {

        /// <summary>
        /// Event triggered when a message is received
        /// </summary>
        public event EventHandler<MessageEventArgs>? MessageReceived;

        /// <summary>
        /// Event triggered when a message is received but with errors
        /// </summary>
        public event EventHandler<MessageErrorEventArgs>? MessageReceivedWithErrors;

        /// <summary>
        /// The Root Path where the listener will store messages
        /// </summary>
        public string StorePhysicalPath { get; set; }

        /// <summary>
        /// The path where received messages are stored
        /// </summary>
        public string StorePhysicalPathReceived { get; }

        /// <summary>
        /// The path where failed messages are stored
        /// </summary>
        public string StorePhysicalPathErrors { get; }

        /// <summary>
        /// The path where rejected messages are stored
        /// </summary>
        public string StorePhysicalPathRejected { get; }

        /// <summary>
        /// Initializes a new instance of the SmtpMessageStore
        /// </summary>
        public SmtpMessageStore(string? storePhysicalPath)
        {
            if (string.IsNullOrWhiteSpace(storePhysicalPath))
                throw new ArgumentNullException(nameof(storePhysicalPath));

            StorePhysicalPath = storePhysicalPath;
            StorePhysicalPathRejected = Path.Combine(StorePhysicalPath, Folders.FILES_LISTENER_REJECTED);
            StorePhysicalPathReceived = Path.Combine(StorePhysicalPath, Folders.FILES_LISTENER_RECEIVED);
            StorePhysicalPathErrors = Path.Combine(StorePhysicalPath, Folders.FILES_LISTENER_ERRORS);
        }

        /// <inheritdoc/>
        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            // The Smtp Message
            var smtpMessage = new SmtpMessage();

            try
            {
                // Retrieve the Mail From
                smtpMessage.Sender = transaction.From.AsAddress();
                smtpMessage.MailFrom = new SmtpMailbox(transaction.From.User, transaction.From.Host);

                // Retrieve the Recipients
                foreach (var mailTo in transaction.To)
                    smtpMessage.Recipients.Add(new SmtpMailbox(mailTo.User, mailTo.Host));

                // Gets the Message Contents
                await using var stream = new MemoryStream();

                var position = buffer.GetPosition(0);
                while (buffer.TryGet(ref position, out var memory))
                {
                    await stream.WriteAsync(memory, cancellationToken);
                }

                stream.Position = 0;
                smtpMessage.Contents = await new StreamReader(stream).ReadToEndAsync(cancellationToken);

                // Receiving Information
                var endpoint = (IPEndPoint)context.Properties[EndpointListener.RemoteEndPointKey];
                smtpMessage.OriginIPAddress = endpoint.Address.ToString();
                smtpMessage.ReceivedByIPAddress = GetLocalIP();
                smtpMessage.ReceivedByHostName = Dns.GetHostName();

                // Save Message to Folder
                if (!string.IsNullOrEmpty(StorePhysicalPathReceived))
                    smtpMessage.SaveToFile(StorePhysicalPathReceived);
                    
                // Trigger Event to inform a message was received
                MessageReceived?.Invoke(this, new MessageEventArgs(smtpMessage));

                return SmtpResponse.Ok;
            }
            catch (Exception e)
            {
                try
                {
                    if (!string.IsNullOrEmpty(StorePhysicalPathErrors))
                        smtpMessage.SaveToFile(StorePhysicalPathErrors);
                }
                finally
                {
                    // Notify listener
                    MessageReceivedWithErrors?.Invoke(this, new MessageErrorEventArgs(smtpMessage, e));
                }

                // Something failed
                return SmtpResponse.TransactionFailed;
            }
        }

        /// <summary>
        /// Retrieves current Local IP
        /// </summary>
        /// <returns>A <see cref="string"/> containing the current IP Address</returns>
        private static string GetLocalIP()
        {
            // The easiest way to get an accurate local IP address is using this logic
            // When running in virtual machines, it's likely it will not retrieve the proper information, that is the reason why this method was used
            // Refer to Stackoverflow https://stackoverflow.com/questions/6803073/get-local-ip-address to have a better understanding
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            socket.Connect("8.8.8.8", 65530);
            return socket.LocalEndPoint is not IPEndPoint endPoint ? string.Empty : endPoint.Address.ToString();
        }
    }
}
