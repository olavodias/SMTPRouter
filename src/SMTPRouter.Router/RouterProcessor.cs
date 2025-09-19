using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router;

/// <summary>
/// Process to get received messages and route to the proper SMTP
/// </summary>
public sealed class RouterProcessor : IProcessor
{
    private readonly Folders? _folders;

    private readonly ILogger<RouterProcessor>? _logger;

    private readonly SemaphoreSlim _folderLockSemaphore = new(1, 1);

    public int MaxRoutingThreads { get; set; }

    public RouterProcessor(int maxRoutingThreads, ILogger<RouterProcessor>? logger, Folders? folders)
    {
        _logger = logger;
        _folders = folders;

        if (maxRoutingThreads < 1) maxRoutingThreads = 1;
        else if (maxRoutingThreads > 10) maxRoutingThreads = 10;

        MaxRoutingThreads = maxRoutingThreads;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        // The routing process consists in reading a message from the "Received" Folder,
        // move it to the "Routing" folder,
        // and apply the routing rules in sequence until one rule is a match.
        // If there is a match, move it to the "InQueue" folder inside
        // the SMTPKey folder, under the "Routed" folder.
        // If there is no match, move it to the "Errors" folder.

        // Each connection will be responsible from processing it from the "InQueue" onwards.

        var routingTasks = new List<Task>();

        for (int i = 0; i < MaxRoutingThreads; i++)
            routingTasks.Add(RouteNextMessageAsync(stoppingToken));

        await Task.WhenAll(routingTasks).ConfigureAwait(false);
    }

    private async Task RouteNextMessageAsync(CancellationToken stoppingToken)
    {
        if (_folders is null) throw new InvalidOperationException("The Folder Configuration is not defined");

        var fileToRoute = string.Empty;
        FileInfo? fileToRouteInfo = null;

        while (!stoppingToken.IsCancellationRequested) {

            // Wait the semaphore to read the file. Only one task can read the directory at once.
            try
            {
                await _folderLockSemaphore.WaitAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                // Get the first file from the directory
                fileToRoute = Directory.EnumerateFiles(_folders.FilesListenerReceived, "*.eml").FirstOrDefault();

                if (fileToRoute is not null)
                {
                    try
                    {
                        fileToRouteInfo = new FileInfo(fileToRoute);

                        File.Move(fileToRoute, Path.Combine(_folders.FilesRouterRouting, fileToRouteInfo.Name));
                    }
                    catch
                    {
                        //TODO: Notify thru logging that something shitty just happened
                        fileToRoute = null;
                    }
                }
            }
            finally
            {
                _folderLockSemaphore.Release();
            }

            // If no file exists, just pause and try again in X seconds
            if (string.IsNullOrWhiteSpace(fileToRoute))
            {
                await Task.Delay(1000, stoppingToken).WaitAsync(stoppingToken);
                continue;
            }

            // Execute routing
            if (fileToRouteInfo is not null)
            {
                //TODO: Check routing rules
                try
                {
                    Task.Delay(1000, stoppingToken).Wait(stoppingToken);

                    File.Move(Path.Combine(_folders.FilesRouterRouting, fileToRouteInfo.Name),
                              Path.Combine(_folders.FilesRouterRouted, fileToRouteInfo.Name));
                }
                finally
                {

                }
            }
        }
    }
}
