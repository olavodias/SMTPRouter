using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router;

/// <summary>
/// Process to get received messages and route to the proper SMTP
/// </summary>
internal class RouterProcessor : IProcessor
{
    private readonly object _folderLock = new();

    private readonly Folders? _folders;

    private readonly ILogger<RouterProcessor>? _logger;

    //TODO: Implment Dispose Pattern because the semaphore needs it
    private readonly SemaphoreSlim? Semaphore;

    public RouterProcessor(int maxConcurrentThreads, ILogger<RouterProcessor>? logger, Folders? folders)
    {
        _logger = logger;
        _folders = folders;

        if (maxConcurrentThreads < 1) maxConcurrentThreads = 1;
        else if (maxConcurrentThreads > 10) maxConcurrentThreads = 10;

        Semaphore = new SemaphoreSlim(0, maxConcurrentThreads);
    }

    public Task DoWorkAsync(CancellationToken stoppingToken)
    {
        // The routing process consists in reading a message from the "Received" Folder,
        // move it to the "Routing" folder,
        // and apply the routing rules in sequence until one rule is a match.
        // If there is a match, move it to the "InQueue" folder inside
        // the SMTPKey folder, under the "Routed" folder.
        // If there is no match, move it to the "Errors" folder.

        // Each connection will be responsible from processing it from the "InQueue" onwards.

        while (!stoppingToken.IsCancellationRequested)
        {
            


        }

        throw new NotImplementedException();
        
    }

    private static async Task RouteNextMessageAsync()
    {

    }
}
