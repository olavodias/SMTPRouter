using Microsoft.Extensions.Logging;
using SMTPRouter.Abstractions;
using SMTPRouter.Core.ConfigurationSchema;
using SMTPRouter.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter.Core;

/// <summary>
/// Process to get received messages and route to the proper SMTP
/// </summary>
public sealed class RouterProcessor : IProcessor
{
    // **********************************************************************
    // Private Properties
    // **********************************************************************

    private readonly ILogger<RouterProcessor>? _logger;
    internal readonly RouterSetup? _routerSetup;
    internal readonly Folders? _folders;

    internal readonly Dictionary<string, RoutingConnection> Connections = new();
    internal readonly Dictionary<string, IRoutingRule> RoutingRules = new();

    private readonly SemaphoreSlim _folderLockSemaphore = new(1, 1);

    // **********************************************************************
    // Routing Thread Count Management
    // **********************************************************************

    /// <summary>
    /// The maximum number of active threads routing messages
    /// </summary>
    /// <remarks>The system limits it between 1 and 10</remarks>
    public int MaxThreadCount { get; }

    private readonly object _threadCountLock = new();
    private byte _threadCount = 0;

    /// <summary>
    /// The number of threads actively running
    /// </summary>
    /// <remarks>It is expected that this value will match the value of <see cref="MaxThreadCount"/></remarks>
    public byte ActiveThreadCount => _threadCount;

    /// <summary>
    /// Increments the number of active threads
    /// </summary>
    private void IncrementActiveThreadCount()
    {
        lock (_threadCountLock)
        {
            _threadCount++;
        }
    }

    /// <summary>
    /// Decrements the number of active threads
    /// </summary>
    private void DecrementActiveThreadCount()
    {
        lock (_threadCountLock)
        {
            _threadCount--;
        }
    }

    // **********************************************************************
    // Constructors
    // **********************************************************************

    /// <summary>
    /// Initializes a new instance of the <see cref="RouterProcessor"/> class
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="routerSetup"></param>
    /// <param name="folders"></param>
    public RouterProcessor(ILogger<RouterProcessor>? logger, RouterSetup? routerSetup, Folders? folders)
    {
        // Setup Readonly Properties
        _logger = logger;
        _routerSetup = routerSetup;
        _folders = folders;

        // Setup Routing Threads (minimum 1, maximum 10)
        _routerSetup ??= new RouterSetup()
        {
            Path = AppContext.BaseDirectory,
            RoutingActiveThreads = 4,
        };

        if (_routerSetup.RoutingActiveThreads < 1) MaxThreadCount = 1;
        else if (_routerSetup.RoutingActiveThreads > 10) MaxThreadCount = 10;
        else MaxThreadCount = _routerSetup.RoutingActiveThreads;
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

    private void SetupRoutingRules()
    {
        if (_routerSetup is null) return;
        if (_routerSetup.Rules is null) return;

        foreach (var r in _routerSetup.Rules)
            RoutingRules.Add(r.Key, RuleFactory.Create(r.Value));
    }

    /// <inheritdoc/>
    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        // The routing process consists in reading a message from the "Received" Folder,
        // move it to the "Routing" folder,
        // and apply the routing rules in sequence until one rule is a match.
        // If there is a match, move it to the "InQueue" folder inside
        // the SMTPKey folder, under the "Routed" folder.
        // If there is no match, move it to the "Errors" folder.

        // Each connection will be responsible from processing it from the "InQueue" onwards.
        // The connection processing happens in a different class.
        try
        {
            SetupConnections();
            SetupRoutingRules();

            var activeTasks = new List<Task>();

            // Add Routing Tasks
            for (int i = 0; i < MaxThreadCount; i++)
                activeTasks.Add(RouteNextMessageAsync(stoppingToken));

            await Task.WhenAll(activeTasks).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Log Error
            _logger?.LogError(LoggingEvents.RoutingErrors, e, "General error in the \"{className}.{functionName}\" method", nameof(RouterProcessor), nameof(DoWorkAsync));
        }
    }

    // **********************************************************************
    // Routing Functionality
    // **********************************************************************

    /// <summary>
    /// Checks for messages waiting to be routed, in sync to avoid concurrency, and attempt to route them
    /// </summary>
    /// <param name="stoppingToken">The canncelation token</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">Thrown if the folders are not setup properly</exception>
    private async Task RouteNextMessageAsync(CancellationToken stoppingToken)
    {
        // Active Thread Count Management
        IncrementActiveThreadCount();
        _logger?.LogInformation("A new thread of the \"{taskName}\" was created. Total active threads now is {count}. Maximum is {countmax}.", nameof(RouteNextMessageAsync), ActiveThreadCount, MaxThreadCount);

        try
        {
            // Check things that cannot be null
            if (_folders is null) throw new NullReferenceException("The Folder Configuration is not defined");

            // Keep looping until a cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                // Main try to prevent the thread from exiting in case of an exception
                try
                {
                    // Get the next file to process (automatically move to the routing folder, if a file was found)
                    var fileToRoute = await GetNextFileNameAsync(stoppingToken);
                    _logger?.LogTrace("File \"{fileToRoute}\" is being processed", fileToRoute);

                    // Load Message from file
                    var message = SmtpMessage.LoadFile(Path.Combine(_folders.FilesRouterRouting, fileToRoute));
                    _logger?.LogTrace("File \"{fileToRoute}\" was sucessfully converted into a SmtpMessage", fileToRoute);

                    // Apply Routing Rules (automatic move to connection InQueue folder, if succesfully routed)
                    ApplyRoutingRules(fileToRoute, message);
                    _logger?.LogTrace("File \"{fileToRoute}\" was routed succesfully", fileToRoute);
                }
                catch (OperationCanceledException)
                {
                    // Thrown when the cancellation token is triggered, exit the while loop
                    throw;
                }
                catch (UnableToRetrieveMessageToRouteException)
                {
                    // There is no message to route, nothing special needs to be done
                }
                catch (UnableToRouteMessageException e)
                {
                    // Log the error
                    _logger?.LogError(LoggingEvents.RoutingErrors, e, "Unable to route a message");

                    try
                    {
                        // Move Unroutable file to the error folder
                        if (!string.IsNullOrWhiteSpace(e.FileToRoute))
                        {
                            MultiAttemptHelper.FileMove(Path.Combine(_folders.FilesRouterRouting, e.FileToRoute),
                                                        Path.Combine(_folders.FilesRouterError, e.FileToRoute));
                        }
                    }
                    catch (Exception e1)
                    {
                        // Log the error
                        _logger?.LogError(LoggingEvents.RoutingErrors, e1, "Unable to move file \"{fileToRoute}\" to the folder \"{folder}\"", e.FileToRoute, _folders.FilesRouterError);
                    }
                }
                catch (Exception e)
                {
                    // Log the error, but stay in the loop
                    _logger?.LogError(LoggingEvents.RoutingErrors, e, "An error occurred while attempting to route a message");
                }

                // Leave in case the cancellation was requested
                stoppingToken.ThrowIfCancellationRequested();

                // Wait to try again
                await Task.Delay(1000, stoppingToken).WaitAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Log Information
            _logger?.LogInformation("The task \"{taskName}\", (Thread ID {taskId}) was cancelled using a cancellation token.", nameof(RouteNextMessageAsync), Task.CurrentId);
        }
        catch (Exception e)
        {
            // Log the error and leave
            _logger?.LogError(LoggingEvents.RoutingErrors, e, "An error ocurred in the \"{taskName}\" (Thread ID {taskId}) causing it to finish unexpectedly", nameof(RouteNextMessageAsync), Task.CurrentId);
        }
        finally
        {
            DecrementActiveThreadCount();
            _logger?.LogInformation("A thread of the \"{taskName}\" was ended. Total active threads now is {count}. Maximum is {countmax}.", nameof(RouteNextMessageAsync), ActiveThreadCount, MaxThreadCount);
        }
    }

    /// <summary>
    /// Gets the next File to route from the Received Folder
    /// </summary>
    /// <remarks>This function uses a Semaphore, preventing concurrent access to the folder</remarks>
    /// <param name="stoppingToken">The cancellation token</param>
    /// <returns>A string containing the file name (not the full path)</returns>
    /// <exception cref="OperationCanceledException">Thrown when a cancellation is triggered</exception>
    /// <exception cref="UnableToRetrieveMessageToRouteException">Thrown if the folder is empty</exception>
    private async Task<string> GetNextFileNameAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Make sure the setups are valid
            if (_folders is null) throw new NullReferenceException("The folders are not setup for the router");

            // Wait the semaphore to read the file. Only one task can read the directory at once.
            await _folderLockSemaphore.WaitAsync(stoppingToken);
            stoppingToken.ThrowIfCancellationRequested();

            // Get the first file from the directory
            var fileToRoute = Directory.EnumerateFiles(_folders.FilesListenerReceived, "*.eml").FirstOrDefault() ??
                              throw new UnableToRetrieveMessageToRouteException($"The folder \"{_folders.FilesListenerReceived}\" is empty");

            // Move it to the Routing Folder
            var fileToRouteInfo = new FileInfo(fileToRoute);
            MultiAttemptHelper.FileMove(fileToRoute, Path.Combine(_folders.FilesRouterRouting, fileToRouteInfo.Name));

            return fileToRouteInfo.Name;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (UnableToRetrieveMessageToRouteException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(LoggingEvents.UnableToRetrieveMessagesToRoute, e, $"Error on {nameof(GetNextFileNameAsync)} function");
            throw;
        }
        finally
        {
            _folderLockSemaphore.Release();
        }
    }

    /// <summary>
    /// Applies Routing Rules until one is a match
    /// </summary>
    /// <remarks>The function will also move the files to the proper folders</remarks>
    /// <param name="fileToRouteNameOnly">The name of the file to route, without the full path</param>
    /// <param name="message">The message to route</param>
    /// <exception cref="UnableToRouteMessageException">Thrown when the system could not route the message. The Inner Exception displayes the cause of the exception.</exception>
    private void ApplyRoutingRules(string fileToRouteNameOnly, SmtpMessage message)
    {
        try
        {
            // Ensure no nulls
            if (_folders is null) throw new NullReferenceException("Folders are not setup");
            if (_routerSetup is null) throw new NullReferenceException("Router Setup is Null");
            if (_routerSetup.Rules is null) throw new NullReferenceException("Router Setup Rules are not defined");

            // Check file is valid
            var fileToRouteFullPath = Path.Combine(_folders.FilesRouterRouting, fileToRouteNameOnly);
            if (!File.Exists(fileToRouteFullPath))
                throw new FileNotFoundException("The File To Route could not be located", fileToRouteFullPath);

            // Test Routing Rules in Sequence
            foreach (var r in RoutingRules)
            {
                if (r.Value.Match(message))
                {
                    // Retrieve the Connection Information
                    if (!_routerSetup.Rules.TryGetValue(r.Key, out var rule))
                        throw new KeyNotFoundException($"A Rule Definition with key \"{r.Key}\" could not be located under the Router Setup Rules");

                    if (string.IsNullOrWhiteSpace(rule.ConnectionKey))
                        throw new NullReferenceException($"The connect \"{r.Key}\" is setup with an null or blank connection key");

                    if (!Connections.TryGetValue(rule.ConnectionKey, out var connection))
                        throw new NullReferenceException($"The connect \"{r.Key}\" uses connection \"{rule.ConnectionKey}\", which could not be located");

                    // Move File to the InQueue folder of the given connection
                    MultiAttemptHelper.FileMove(fileToRouteFullPath,
                                                Path.Combine(connection.Folders.FilesConnectionInQueue, fileToRouteNameOnly));

                    // Finalize Function execution
                    return;
                }
            }

            // Unable to apply a rule
            throw new UnableToRouteMessageException(message, fileToRouteNameOnly, "The SmtpMessage does not match any of the routing rules");
        }
        catch (Exception e)
        {
            throw new UnableToRouteMessageException(message, fileToRouteNameOnly, e);
        }
    }
}
