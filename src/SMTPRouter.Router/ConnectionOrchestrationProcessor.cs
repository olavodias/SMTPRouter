using SMTPRouter.ConfigurationSchema;
using SMTPRouter.RoutingRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router;

public sealed class ConnectionOrchestrationProcessor: IProcessor
{
    // **********************************************************************
    // Private Properties
    // **********************************************************************

    private readonly ILogger<ConnectionOrchestrationProcessor>? _logger;
    private readonly RouterSetup? _routerSetup;
    private readonly Folders? _folders;

    private readonly Dictionary<string, RoutingConnection> Connections = [];

    // **********************************************************************
    // Constructors
    // **********************************************************************

    public ConnectionOrchestrationProcessor(ILogger<ConnectionOrchestrationProcessor>? logger, RouterSetup? routerSetup, Folders? folders)
    {
        // Setup Readonly Properties
        _logger = logger;
        _routerSetup = routerSetup;
        _folders = folders;

        // Setup Router Configuration
        _routerSetup ??= new RouterSetup()
        {
            Path = AppContext.BaseDirectory            
        };
    }

    // **********************************************************************
    // Setups Prior to Execution
    // **********************************************************************

    private void SetupConnections()
    {
        if (_routerSetup is null) return;
        if (_routerSetup.Connections is null) return;
        if (_folders is null) return;

        foreach (var c in _routerSetup.Connections)
        {
            Connections.Add(c.Key, new RoutingConnection(c.Key, new Folders(_folders.RootPath, c.Key), c.Value));
        }
    }

    // **********************************************************************
    // Orchestration Functionality
    // **********************************************************************

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        // The Orchestration will call the ConnectionProcessor for each one of the
        // connections. This will create the appropriate number of threads
        // and perform the email execution.

        SetupConnections();

        var activeTasks = new List<Task>();

        // Add Connection Tasks
        foreach (var c in Connections)
        {
            var connectionProcessor = new ConnectionProcessor(_logger, c.Value);
            activeTasks.Add(connectionProcessor.DoWorkAsync(stoppingToken));
        }

        await Task.WhenAll(activeTasks).ConfigureAwait(false);
    }

}
