#pragma warning disable IDE0060 // Remove unused parameter

using MimeKit;
using MailKit.Net.Smtp;
using SMTPRouter.ConfigurationSchema;
using SMTPRouter.Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.UnitTesting;

[TestClass]
[DoNotParallelize]
public sealed class TestConnectionProcessor
{
    // This test consists in verifying if a Connection Processor works.
    // The test will require the following objects:
    // Main SMTP => Port 9025
    //      This will receive a message and route it to SMTP01
    //      Then, the Connection Processor will send the message to the SMTP01.
    // SMTP01    => Port 9125
    //      This will receive a message and keep it on the Received Folder

    public static string RootFolder { get; } = Path.Combine(Directory.GetCurrentDirectory(), nameof(TestConnectionProcessor));

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        // Cleanup Existing Testing if any garbage is left
        if (Directory.Exists(RootFolder))
            Directory.Delete(RootFolder, true);

        // Recreate the folder
        Directory.CreateDirectory(RootFolder);
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void ClassCleanup()
    {
        if (Directory.Exists(RootFolder))
            Directory.Delete(RootFolder, true);
    }

    // Variables exclusive for the testing
    private CancellationTokenSource cts = new();
    private ListenerProcessor? ListenerMain;
    private ListenerProcessor? ListenerSMTP01;
    private string? CurrentTestRootFolder;
    private RouterProcessor? MainRouter;
    private ConnectionProcessor? ConnectionProcessor;

    [TestInitialize]
    public void TestInitialize()
    {
        // Create a Unique Folder for the Testing
        int number = Random.Shared.Next(99999) + 1;
        CurrentTestRootFolder = Path.Combine(RootFolder, $"t{number:00000}");

        // Main Cancellation Token
        cts = new CancellationTokenSource();

        // Setup Main Listener and Activate It
        ListenerMain = new ListenerProcessor(null, new ConfigurationSchema.Hosting()
        {
            Server = "localhost",
            Path = Path.Combine(CurrentTestRootFolder, nameof(ListenerMain)),
            PortsConfiguration = new()
            {
                { "Default", new PortConfiguration(9025, false) }
            }
        }, new Folders(Path.Combine(CurrentTestRootFolder, nameof(ListenerMain))));

        Task.Run(async () => await ListenerMain.DoWorkAsync(cts.Token));

        // Setup Structure for the SMTP01
        ListenerSMTP01 = new ListenerProcessor(null, new ConfigurationSchema.Hosting()
        {
            Server = "localhost",
            Path = Path.Combine(CurrentTestRootFolder, nameof(ListenerSMTP01)),
            PortsConfiguration = new()
            {
                { "Default", new PortConfiguration(9125, false) }
            }
        }, new Folders(Path.Combine(CurrentTestRootFolder, nameof(ListenerSMTP01))));

        Task.Run(async () => await ListenerSMTP01.DoWorkAsync(cts.Token));

        // Setup Router for ListenerMain
        MainRouter = new RouterProcessor(null, new RouterSetup()
        {
            RoutingActiveThreads = 1,
            Path = Path.Combine(CurrentTestRootFolder, nameof(ListenerMain)),
            Connections = new()
            {
                { "SMTP01",
                    new Connection()
                    {
                        ActiveConnections = 1,
                        Description = "Smtp 01",
                        Host = "localhost",
                        Port = 9125,
                        RequiresAuthentication = false,
                        GroupingOption = GroupingOptions.NoGrouping
                    }
                },
                { "SMTP02",
                    new Connection()
                    {
                        ActiveConnections = 1,
                        Description = "Smtp 02",
                        Host = "localhost",
                        Port = 9225,
                        RequiresAuthentication = false,
                        GroupingOption = GroupingOptions.NoGrouping
                    }
                }
            },
            Rules = new()
            {
                { "Rule01",
                    new Rule()
                    {
                        ConnectionKey = "SMTP01",
                        Type = "MailFromDomainRoutingRule",
                        Parameters = new()
                        {
                            { "Domain", "smtp01.com" }
                        }
                    }
                },
                { "Rule02",
                    new Rule()
                    {
                        ConnectionKey = "SMTP02",
                        Type = "MailFromDomainRoutingRule",
                        Parameters = new()
                        {
                            { "Domain", "smtp02.com" }
                        }
                    }
                },
            }
        }, new Folders(Path.Combine(CurrentTestRootFolder, nameof(ListenerMain))));

        Task.Run(async () => await MainRouter.DoWorkAsync(cts.Token));

        // Setup Connection Processor for SMTP01
        Assert.IsNotNull(MainRouter._routerSetup);
        Assert.IsNotNull(MainRouter._routerSetup.Connections);

        var smtp01Connection = MainRouter._routerSetup.Connections["SMTP01"];
        Assert.IsNotNull(smtp01Connection);

        ConnectionProcessor = new ConnectionProcessor(null,
                                                      new RoutingConnection("SMTP01",
                                                                            new Folders(Path.Combine(CurrentTestRootFolder, nameof(ListenerMain)), "SMTP01"),
                                                                            smtp01Connection));

        Task.Run(async () => await ConnectionProcessor.DoWorkAsync(cts.Token));
    }

    [TestCleanup]
    public void TestCleanup()
    {
        // Clean Folder
        if (Directory.Exists(CurrentTestRootFolder))
            Directory.Delete(CurrentTestRootFolder, true);

        cts?.Cancel();

        Task.Delay(1000).Wait();

        cts?.Dispose();
    }

    private void SendEmail(string sourceSender)
    {
        // Send an email to the new SMTP
        using var smtpClient = new SmtpClient();
        smtpClient.Connect("localhost", 9025, MailKit.Security.SecureSocketOptions.Auto);

        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sender", sourceSender));
        message.To.Add(new MailboxAddress("Recipient", "recipient@test.com"));

        message.Subject = $"{nameof(TestConnectionProcessor)} Test Email";

        var messageBodyBuilder = new BodyBuilder
        {
            TextBody = $"This is a test email generated by method ${nameof(TestConnectionProcessor)}"
        };

        message.Body = messageBodyBuilder.ToMessageBody();

        smtpClient.Send(FormatOptions.Default, message, cts.Token);
    }

    [TestMethod]
    public void TestConnectionProcessing()
    {
        // Check For Minimal Non Nulls
        Assert.IsNotNull(ListenerMain);
        Assert.IsNotNull(ListenerMain._folders);

        Assert.IsNotNull(ListenerSMTP01);
        Assert.IsNotNull(ListenerSMTP01._folders);

        // Send Emails
        SendEmail("sender@smtp01.com");
        SendEmail("sender@smtp01.com");

        SendEmail("sender@smtp02.com");

        // Give it 5 seconds to process (which is plenty of time)
        Task.Delay(5000).Wait();

        // Look for 1 messages on the InQueue Folder of SMTP02
        var folderToInspect = Path.Combine(ListenerMain._folders.FilesRouterRouted, "SMTP02", Folders.FILES_CONNECTION_IN_QUEUE);
        Assert.AreEqual(1, Directory.GetFiles(folderToInspect).Length);

        // Look for 2 messages on the Sent Folder of the Main SMTP
        folderToInspect = Path.Combine(ListenerMain._folders.FilesRouterRouted, "SMTP01", Folders.FILES_CONNECTION_SENT);
        Assert.AreEqual(2, Directory.GetFiles(folderToInspect).Length);

        // Look for 2 messages on the Received of SMTP01
        folderToInspect = ListenerSMTP01._folders.FilesListenerReceived;
        Assert.AreEqual(2, Directory.GetFiles(folderToInspect).Length);


    }

}

#pragma warning restore IDE0060 // Remove unused parameter
