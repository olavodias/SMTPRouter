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
public sealed class IntegratedTest
{
    // The integrated test consists in the creation of 3 SMTPs.
    // Main SMTP => Port 9025
    //      This will receive messages and route to SMTP01 or SMTP02
    // SMTP01    => Port 9125
    //      This will receive messages, but no routing will exists, it will sit on the Received Folder
    // SMTP02    => Port 9225
    //      This will receive messages, but no routing will exists, it will sit on the Received Folder

    public static string RootFolder { get; } = Path.Combine(Directory.GetCurrentDirectory(), "SMTPRouterIntegratedTesting");

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

    // Variables exclusive for each one of the tests
    private CancellationTokenSource cts = new();
    private ListenerProcessor? ListenerMain;
    private ListenerProcessor? ListenerSMTP01;
    private ListenerProcessor? ListenerSMTP02;
    private string? CurrentTestRootFolder;
    private RouterProcessor? MainRouter;


    [TestInitialize]
    public void TestInitialize()
    {
        // Create a Unique Folder for the Testing
        int number = Random.Shared.Next(99999) + 1;
        CurrentTestRootFolder = Path.Combine(RootFolder, $"t{number:00000}");

        // Main Cancellation Token
        cts = new CancellationTokenSource();

        // Setup and Run Listeners
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

        ListenerSMTP02 = new ListenerProcessor(null, new ConfigurationSchema.Hosting()
        {
            Server = "localhost",
            Path = Path.Combine(CurrentTestRootFolder, nameof(ListenerSMTP02)),
            PortsConfiguration = new()
            {
                { "Default", new PortConfiguration(9225, false) }
            }
        }, new Folders(Path.Combine(CurrentTestRootFolder, nameof(ListenerSMTP02))));

        Task.Run(async () => await ListenerSMTP02.DoWorkAsync(cts.Token));

        // Setup Router
        MainRouter = new RouterProcessor(null, new RouterSetup()
        {
            RoutingActiveThreads = 1,
            Path = Path.Combine(CurrentTestRootFolder, nameof(ListenerMain)),
            Connections = new()
            {
                { "SMTP01",
                    new Connection()
                    {
                        ActiveConnections = 2,
                        Description = "Smtp 01",
                        Host = "localhost",
                        Port = 9125,
                        RequiresAuthentication = false,
                        GroupingOption = 2
                    }
                },
                { "SMTP02",
                    new Connection()
                    {
                        ActiveConnections = 2,
                        Description = "Smtp 02",
                        Host = "localhost",
                        Port = 9225,
                        RequiresAuthentication = false,
                        GroupingOption = 2
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

        message.Subject = "Integrated Test Email";

        var messageBodyBuilder = new BodyBuilder
        {
            TextBody = "This is a test email generating during Integrated Testing"
        };

        message.Body = messageBodyBuilder.ToMessageBody();

        smtpClient.Send(FormatOptions.Default, message, cts.Token);
    }

    [TestMethod]
    public void TestRoutingProcess()
    {
        SendEmail("sender@smtp01.com");
        SendEmail("sender@smtp01.com");
        
        SendEmail("sender@smtp02.com");

        Task.Delay(20000).Wait();

        // Look for 2 messages on the Routing Folder of SMTP01
        Assert.IsNotNull(ListenerSMTP01);
        Assert.IsNotNull(ListenerSMTP01._folders);
        var folderToInspect = ListenerSMTP01._folders.FilesRouterRouting;

        Assert.AreEqual(2, Directory.GetFiles(folderToInspect).Length);

        // Look for 1 messages on the Routing Folder of SMTP01
        Assert.IsNotNull(ListenerSMTP02);
        Assert.IsNotNull(ListenerSMTP02._folders);
        folderToInspect = ListenerSMTP02._folders.FilesRouterRouting;

        Assert.AreEqual(1, Directory.GetFiles(folderToInspect).Length);

    }

}

#pragma warning restore IDE0060 // Remove unused parameter
