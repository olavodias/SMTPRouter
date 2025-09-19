using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMTPRouter;
using SMTPRouter.ConfigurationSchema;
using SMTPRouter.Listener;
using System;
using System.Diagnostics;
using System.IO;
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
        if (!EventLog.SourceExists(serviceName))
            EventLog.CreateEventSource(serviceName, "SMTPRouter Service");

        options.AddEventLog(eventLog => {
            
            eventLog.SourceName = serviceName;
        });
    }

#pragma warning restore CA1416 // Validate platform compatibility

});

// Make it a Service
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

// Add the Hosting Information
//TODO: maybe add an extension method "AddSmtpRouterHosting"
builder.Services.AddSingleton<Hosting>(provider =>
{
    var configurationFileName = Path.Combine(AppContext.BaseDirectory, "listener.json");
    Hosting? hosting = null;

    if (File.Exists(configurationFileName))
        hosting = JsonSerializer.Deserialize<Hosting>(System.IO.File.ReadAllText(configurationFileName, Encoding.UTF8));

    hosting ??= new Hosting()
    {
        Server = "localhost",
        PortsConfiguration = new System.Collections.Generic.Dictionary<string, PortConfiguration>
        {
            { "Default", new PortConfiguration(25, false) }
        },
        Path = AppContext.BaseDirectory,
    };

    return hosting;
});

// Add the Folders
builder.Services.AddSingleton<Folders>(provider =>
{
    var hosting = provider.GetRequiredService<Hosting>();
    var path = hosting is null ? AppContext.BaseDirectory : 
                                (hosting.Path is null ? AppContext.BaseDirectory : 
                                                        hosting.Path);

    return new Folders(path);
});

// Add the Processor
builder.Services.AddSingleton<IProcessor>(provider =>
{
    return new ListenerProcessor(provider.GetRequiredService<ILogger<ListenerProcessor>>(), provider.GetRequiredService<Hosting>(), provider.GetRequiredService<Folders>());
});

// Add the Worker to call the processor
builder.Services.AddHostedService<Worker>();

// Build and Run the host
var host = builder.Build();
host.Run();
