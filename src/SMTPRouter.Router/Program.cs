using SMTPRouter;
using SMTPRouter.ConfigurationSchema;
using SMTPRouter.Router;
using System.Reflection;
using System.Text;
using System.Text.Json;

// Get Assembly Information for Service / EventLog Registration
var currentAssembly = Assembly.GetExecutingAssembly() ?? throw new Exception("Executing Assembly Is Null");
var currentAssemblyName = currentAssembly.GetName() ?? throw new Exception("Executing Assembly Name Is Null");
var currentAssemblyVersion = currentAssemblyName.Version ?? throw new Exception("Executing Assembly Version Is Null");

var attributes = currentAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
var currentAssemblyProductName = "SMTPRouter.Listener";

if (attributes is not null && attributes.Length > 0)
    currentAssemblyProductName = ((AssemblyProductAttribute)attributes[0]).Product;

var serviceName = $"{currentAssemblyProductName}_v{currentAssemblyVersion}";

// Create the Host
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging(options => {
    options.ClearProviders();
    options.AddConsole();

#pragma warning disable CA1416 // Validate platform compatibility

    if (OperatingSystem.IsWindows())
    {
        options.AddEventLog(eventLog => {
            eventLog.SourceName = serviceName;
        });
    }

#pragma warning restore CA1416 // Validate platform compatibility

});

if (OperatingSystem.IsWindows())
{
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = serviceName;
    });
}

if (OperatingSystem.IsLinux())
{
    builder.Services.AddSystemd();
}

// Add the Configuration
builder.Services.AddSingleton<RouterSetup>(provider => {

    var configurationFileName = Path.Combine(AppContext.BaseDirectory, "router.json");
    RouterSetup? routerSetup = null;

    if (File.Exists(configurationFileName))
        routerSetup = JsonSerializer.Deserialize<RouterSetup>(System.IO.File.ReadAllText(configurationFileName, Encoding.UTF8));

    routerSetup ??= new RouterSetup()
    {
        Path = AppContext.BaseDirectory,
    };

    return routerSetup;
});

// Add the Folders
builder.Services.AddSingleton<Folders>(provider => {

    var routerSetup = provider.GetRequiredService<RouterSetup>();
    var path = (routerSetup is null ? AppContext.BaseDirectory :
                                      (routerSetup.Path is null ? AppContext.BaseDirectory :
                                                                  routerSetup.Path));

    return new Folders(path);
});

// Add Processors
builder.Services.AddSingleton<RouterProcessor>(provider => {
    return new RouterProcessor(4, provider.GetRequiredService<ILogger<RouterProcessor>>(), provider.GetRequiredService<Folders>());
});

// Add the Worker to call the processors
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
