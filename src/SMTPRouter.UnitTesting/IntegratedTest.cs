using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMTPRouter.ConfigurationSchema;

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
        if (!Directory.Exists(RootFolder))
            Directory.CreateDirectory(RootFolder);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        if (Directory.Exists(RootFolder))
            Directory.Delete(RootFolder, true);
    }

    // Variables exclusive for each one of the tests
    private CancellationTokenSource? cts;
    private ListenerProcessor? ListenerMain;
    private ListenerProcessor? ListenerSMTP01;
    private ListenerProcessor? ListenerSMTP02;
    private string? CurrentTestRootFolder;


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
            Path = Path.Combine(CurrentTestRootFolder, nameof(ListenerMain)),
            PortsConfiguration = new()
            {
                { "Default", new PortConfiguration(9125, false) }
            }
        }, new Folders(Path.Combine(CurrentTestRootFolder, nameof(ListenerSMTP01))));

        Task.Run(async () => await ListenerSMTP01.DoWorkAsync(cts.Token));

        ListenerSMTP02 = new ListenerProcessor(null, new ConfigurationSchema.Hosting()
        {
            Server = "localhost",
            Path = Path.Combine(CurrentTestRootFolder, nameof(ListenerMain)),
            PortsConfiguration = new()
            {
                { "Default", new PortConfiguration(9225, false) }
            }
        }, new Folders(Path.Combine(CurrentTestRootFolder, nameof(ListenerSMTP01))));

        Task.Run(async () => await ListenerSMTP02.DoWorkAsync(cts.Token));

        // Setup Router
        //TODO: CONTINUE FROM HERE
    }

    [TestCleanup]
    public void TestCleanup()
    {
        // Clean Folder
        if (Directory.Exists(CurrentTestRootFolder))
            Directory.Delete(CurrentTestRootFolder, true);
    }

    [TestMethod]
    public void TestRoutingProcess()
    {

    }

}
