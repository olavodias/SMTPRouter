using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMTPRouter;
using SMTPRouter.Listener;
using System;
using System.Reflection;

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

builder.Services.AddSingleton<IProcessor>(provider =>
{
    return new ListenerProcessor(provider.GetRequiredService<ILogger<ListenerProcessor>>());
});
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
