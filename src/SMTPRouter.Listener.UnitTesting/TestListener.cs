#pragma warning disable IDE0060 // Remove unused parameter

using SMTPRouter.ConfigurationSchema;
using SMTPRouter.Listener;
using MailKit.Net.Smtp;
using MimeKit;
using System.Diagnostics;

namespace SMTPRouter.Listener.UnitTesting
{
    [TestClass]
    [DoNotParallelize]
    public sealed class TestListener
    {
        public static string RootFolder { get; } = Path.Combine(Directory.GetCurrentDirectory(), "SMTPRouterTesting");

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            if (!Directory.Exists(RootFolder))
                Directory.CreateDirectory(RootFolder);
        }

        [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
        public static void ClassCleanup() 
        {
            if (Directory.Exists(RootFolder))
                Directory.Delete(RootFolder, true);
        }

        private string testRootFolder = string.Empty;
        private string outgoingFolder = string.Empty;
        private ListenerProcessor listener = new();
        private CancellationTokenSource cts = new();


        [TestInitialize]
        public void TestInitialize()
        {
            // Create a Unique Folder for the Testing
            int number = Random.Shared.Next(99999) + 1;
            testRootFolder = Path.Combine(RootFolder, $"t{number:00000}");

            // Destroy anything if by any chance there is data inside it, and recreate it
            if (Directory.Exists(testRootFolder))
                Directory.Delete(testRootFolder, true);

            Directory.CreateDirectory(testRootFolder);

            // Setup Outgoing Folder
            outgoingFolder = Path.Combine(testRootFolder, ListenerProcessor.PATH_QUEUES, "Outgoing");

            Directory.CreateDirectory(outgoingFolder);

            // Create the Local Listener
            listener = new ListenerProcessor
            {
                Hosting = new ConfigurationSchema.Hosting()
                {
                    Server = "localhost",
                    Path = testRootFolder,
                    PortsConfiguration = new()
                    {
                        { "Default", new PortConfiguration(25, false) }
                    }
                },
            };

            // Recreate Token Source
            cts = new CancellationTokenSource();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Clean Folder
            if (Directory.Exists(testRootFolder))
                Directory.Delete(testRootFolder, true);

            cts?.Cancel();

            Task.Delay(1000).Wait();

            cts?.Dispose();
        }

        [TestMethod]
        public void TestListenerProcessor_ReceiveEmail()
        {
            try
            {
                // Start Listener Concurrently
                Task.Run(async () => await listener.DoWorkAsync(cts.Token));

                // Wait a few seconds before trying to send an email
                Task.Delay(1000, cts.Token).Wait();

                // Send an email to the new SMTP
                using var smtpClient = new SmtpClient();
                smtpClient.Connect(listener.Hosting?.Server, 25, MailKit.Security.SecureSocketOptions.Auto);

                using var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sender", "sender@test.com"));
                message.To.Add(new MailboxAddress("Recipient", "recipient@test.com"));

                message.Subject = "Unit Test Email";

                var messageBodyBuilder = new BodyBuilder
                {
                    TextBody = "This is a test email generating during Unit Testing"
                };

                message.Body = messageBodyBuilder.ToMessageBody();

                smtpClient.Send(FormatOptions.Default, message, cts.Token);

                // Wait a few seconds to check for intercepted messages
                Task.Delay(2000, cts.Token).Wait();

                // Check if there is something in the Outgoing Folder
                var files = Directory.GetFiles(outgoingFolder);

                Assert.IsNotNull(files);
                Assert.AreEqual(1, files.Length);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod]
        public void TestListenerProcessor_ReceiveEmail_AcceptLocalHost()
        {
            // Setup the Listener
            if (listener.Hosting is null) throw new ArgumentNullException(nameof(Hosting));
            listener.Hosting.AcceptedIPAddresses = ["127.0.0.1"];

            try
            {
                // Start Listener Concurrently
                Task.Run(async () => await listener.DoWorkAsync(cts.Token));

                // Wait a few seconds before trying to send an email
                Task.Delay(1000, cts.Token).Wait();

                // Send an email to the new SMTP
                using var smtpClient = new SmtpClient();
                smtpClient.Connect(listener.Hosting?.Server, 25, MailKit.Security.SecureSocketOptions.Auto);

                using var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sender", "sender@test.com"));
                message.To.Add(new MailboxAddress("Recipient", "recipient@test.com"));

                message.Subject = "Unit Test Email";

                var messageBodyBuilder = new BodyBuilder
                {
                    TextBody = "This is a test email generating during Unit Testing"
                };

                message.Body = messageBodyBuilder.ToMessageBody();

                smtpClient.Send(FormatOptions.Default, message, cts.Token);

                // Wait a few seconds to check for intercepted messages
                Task.Delay(2000, cts.Token).Wait();

                // Check if there is something in the Outgoing Folder
                var files = Directory.GetFiles(outgoingFolder);

                Assert.IsNotNull(files);
                Assert.AreEqual(1, files.Length);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod]
        public void TestListenerProcessor_ReceiveEmail_AcceptAnyOtherHost()
        {
            // Setup the Listener
            if (listener.Hosting is null) throw new ArgumentNullException(nameof(Hosting));
            listener.Hosting.AcceptedIPAddresses = ["169.254.1.1"];

            try
            {
                // Start Listener Concurrently
                Task.Run(async () => await listener.DoWorkAsync(cts.Token));

                // Wait a few seconds before trying to send an email
                Task.Delay(1000, cts.Token).Wait();

                // Send an email to the new SMTP
                using var smtpClient = new SmtpClient();
                smtpClient.Connect(listener.Hosting.Server, 25, MailKit.Security.SecureSocketOptions.Auto);

                using var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sender", "sender@test.com"));
                message.To.Add(new MailboxAddress("Recipient", "recipient@test.com"));

                message.Subject = "Unit Test Email";

                var messageBodyBuilder = new BodyBuilder
                {
                    TextBody = "This is a test email generating during Unit Testing"
                };

                message.Body = messageBodyBuilder.ToMessageBody();

                try
                {
                    smtpClient.Send(FormatOptions.Default, message, cts.Token);
                }
                catch (SmtpCommandException)
                {

                }
                catch (Exception)
                {
                    throw;
                }

                // Wait a few seconds to check for intercepted messages
                Task.Delay(2000, cts.Token).Wait();

                // Check if there is something in the Outgoing Folder
                var files = Directory.GetFiles(outgoingFolder);

                Assert.IsNotNull(files);
                Assert.AreEqual(0, files.Length);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod]
        public void TestListenerProcessor_ReceiveEmail_RejectLocalHost()
        {
            // Setup the Listener
            if (listener.Hosting is null) throw new ArgumentNullException(nameof(Hosting));
            listener.Hosting.RejectedIPAddresses = ["127.0.0.1"];

            try
            {
                // Start Listener Concurrently
                Task.Run(async () => await listener.DoWorkAsync(cts.Token));

                // Wait a few seconds before trying to send an email
                Task.Delay(1000, cts.Token).Wait();

                // Send an email to the new SMTP
                using var smtpClient = new SmtpClient();
                smtpClient.Connect(listener.Hosting.Server, 25, MailKit.Security.SecureSocketOptions.Auto);

                using var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sender", "sender@test.com"));
                message.To.Add(new MailboxAddress("Recipient", "recipient@test.com"));

                message.Subject = "Unit Test Email";

                var messageBodyBuilder = new BodyBuilder
                {
                    TextBody = "This is a test email generating during Unit Testing"
                };

                message.Body = messageBodyBuilder.ToMessageBody();

                try
                {
                    smtpClient.Send(FormatOptions.Default, message, cts.Token);
                }
                catch (SmtpCommandException)
                {

                }
                catch (Exception)
                {
                    throw;
                }

                // Wait a few seconds to check for intercepted messages
                Task.Delay(2000, cts.Token).Wait();

                // Check if there is something in the Outgoing Folder
                var files = Directory.GetFiles(outgoingFolder);

                Assert.IsNotNull(files);
                Assert.AreEqual(0, files.Length);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod]
        public void TestListenerProcessor_ReceiveEmail_RejectAnyOtherHost()
        {
            // Setup the Listener
            if (listener.Hosting is null) throw new ArgumentNullException(nameof(Hosting));
            listener.Hosting.RejectedIPAddresses = ["169.254.1.1"];

            try
            {
                // Start Listener Concurrently
                Task.Run(async () => await listener.DoWorkAsync(cts.Token));

                // Wait a few seconds before trying to send an email
                Task.Delay(1000, cts.Token).Wait();

                // Send an email to the new SMTP
                using var smtpClient = new SmtpClient();
                smtpClient.Connect(listener.Hosting.Server, 25, MailKit.Security.SecureSocketOptions.Auto);

                using var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sender", "sender@test.com"));
                message.To.Add(new MailboxAddress("Recipient", "recipient@test.com"));

                message.Subject = "Unit Test Email";

                var messageBodyBuilder = new BodyBuilder
                {
                    TextBody = "This is a test email generating during Unit Testing"
                };

                message.Body = messageBodyBuilder.ToMessageBody();

                try
                {
                    smtpClient.Send(FormatOptions.Default, message, cts.Token);
                }
                catch (SmtpCommandException)
                {

                }
                catch (Exception)
                {
                    throw;
                }

                // Wait a few seconds to check for intercepted messages
                Task.Delay(2000, cts.Token).Wait();

                // Check if there is something in the Outgoing Folder
                var files = Directory.GetFiles(outgoingFolder);

                Assert.IsNotNull(files);
                Assert.AreEqual(1, files.Length);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
