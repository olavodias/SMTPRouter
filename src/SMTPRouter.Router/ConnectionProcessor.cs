using MailKit.Net.Smtp;
using MimeKit;
using SMTPRouter.Router.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRouter.Router;

/// <summary>
/// Process to get emails from the InQueue Folder in the connection and send them
/// </summary>
public sealed class ConnectionProcessor: IProcessor
{
    private readonly ILogger? _logger;
    private readonly RoutingConnection _routingConnection;

    private readonly SmtpClient _client;

    private readonly SemaphoreSlim _folderLockSemaphore = new(1, 1);

    // **********************************************************************
    // Thread Count Management
    // **********************************************************************

    /// <summary>
    /// The maximum number of active threads sending emails for the given connection
    /// </summary>
    /// <remarks>The system limits it to 10</remarks>
    public byte MaxThreadCount { get; }

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
    /// Initializes a new instance of the <see cref="ConnectionProcessor"/> class
    /// </summary>
    /// <param name="logger">The Logger</param>
    /// <param name="routingConnection">The Routing Connection</param>
    public ConnectionProcessor(ILogger? logger, RoutingConnection routingConnection)
    {
        // Setup Readonly Properties
        _logger = logger;
        _routingConnection = routingConnection;

        MaxThreadCount = Convert.ToByte(routingConnection.ConnectionInfo.ActiveConnections);

        if (MaxThreadCount <= 0) MaxThreadCount = 1;
        else if (MaxThreadCount > 10) MaxThreadCount = 10;

        // Creat the Reusable Client
        _client = new();
    }

    /// <inheritdoc/>
    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        // The Connection Processor consists in reading a message from the "InQueue" folder,
        // move it to the "Sending" folder, and attempt to send it.

        // Create the Reusable SmtpClient
        try
        {
            var activeTasks = new List<Task>();

            // Add emailing tasks
            for (int i = 0; i < _routingConnection.ConnectionInfo.ActiveConnections; i++)
                activeTasks.Add(EmailNextMessageAsync(stoppingToken));

            await Task.WhenAll(activeTasks).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            // Log Exception
            _logger?.LogError(LoggingEvents.FileIOError, e, "General Error on the \"{class}.{method}\"", nameof(ConnectionProcessor), nameof(DoWorkAsync));
        }

    }

    // **********************************************************************
    // Email Sending Functionality
    // **********************************************************************

    /// <summary>
    /// Check for messages in the InQueue folder, move them to Sending, and attempt to send the message
    /// </summary>
    /// <param name="stoppingToken">The cancellation token</param>
    /// <returns></returns>
    private async Task EmailNextMessageAsync(CancellationToken stoppingToken)
    {
        // Active Thread Count Management
        IncrementActiveThreadCount();
        _logger?.LogInformation("A new thread of the \"{taskName}\" was created. Total active threads now is {count}. Maximum is {countmax}.", nameof(EmailNextMessageAsync), ActiveThreadCount, MaxThreadCount);

        // Create a client for the given thread
        var client = new SmtpClient();

        try
        {

            // Keep looping until a cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                // Main try to prevent the thread from exiting in case of an exception
                try
                {
                    // Get the next file to process (automatically move to the routing folder, if a file was found)
                    var fileToSend = await GetNextFileNameAsync(stoppingToken);
                    _logger?.LogTrace("File \"{fileToSend}\" is being processed", fileToSend);

                    // Load Message from file
                    var message = SmtpMessage.LoadFile(Path.Combine(_routingConnection.Folders.FilesConnectionSending, fileToSend));
                    _logger?.LogTrace("File \"{fileToSend}\" was sucessfully converted into a SmtpMessage", fileToSend);

                    // Send Message
                    await SendMessageAsync(client, fileToSend, message);
                }
                catch (OperationCanceledException)
                {
                    // Thrown when the cancellation token is triggered, exit the while loop
                    throw;
                }
                catch (UnableToRetrieveMessageToSendException)
                {
                    // There is no message to send, nothing needs to be done
                }
                catch (UnableToSendMessageException e)
                {
                    // Log the error
                    _logger?.LogError(LoggingEvents.UnableToSendMessage, e, "Unable to send message");
                    
                    try
                    {
                        // Move the unsent file to the error folder
                        if (!string.IsNullOrEmpty(e.FileToSend))
                        {
                            MultiAttemptHelper.FileMove(Path.Combine(_routingConnection.Folders.FilesConnectionSending, e.FileToSend),
                                                        Path.Combine(_routingConnection.Folders.FilesConnectionErrors, e.FileToSend));
                        }
                    }
                    catch (Exception e1)
                    {
                        // Log the error
                        _logger?.LogError(LoggingEvents.UnableToSendMessage, e1, "Unable to move file \"{fileToSend}\" to the folder \"{folder}\"", e.FileToSend, _routingConnection.Folders.FilesConnectionErrors);
                    }
                }
                catch (Exception e)
                {
                    // Log the error, but stay in the loop
                    _logger?.LogError(LoggingEvents.UnableToSendMessage, e, "An error occurred while attempting to send a message");
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
            _logger?.LogInformation("The task \"{taskName}\", (Thread ID {taskId}) was cancelled using a cancellation token.", nameof(EmailNextMessageAsync), Task.CurrentId);
        }
        catch (Exception e)
        {
            // Log the error and leave
            _logger?.LogError(LoggingEvents.RoutingErrors, e, "An error ocurred in the \"{taskName}\" (Thread ID {taskId}) causing it to finish unexpectedly", nameof(EmailNextMessageAsync), Task.CurrentId);
        }
        finally
        {
            client?.Dispose();
            DecrementActiveThreadCount();
            _logger?.LogInformation("A thread of the \"{taskName}\" was ended. Total active threads now is {count}. Maximum is {countmax}.", nameof(EmailNextMessageAsync), ActiveThreadCount, MaxThreadCount);
        }
    }

    /// <summary>
    /// Gets the next File to send from the InQueue Folder
    /// </summary>
    /// <remarks>This function uses a Semaphore, preventing concurrent access to the folder</remarks>
    /// <param name="stoppingToken">The cancellation token</param>
    /// <returns>A string containing the file name (not the full path)</returns>
    /// <exception cref="OperationCanceledException">Thrown when a cancellation is triggered</exception>
    /// <exception cref="UnableToRetrieveMessageToSendException">Thrown if the folder is empty</exception>
    private async Task<string> GetNextFileNameAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Wait the semaphore to read the file. Only one task can read the directory at once.
            await _folderLockSemaphore.WaitAsync(stoppingToken);
            stoppingToken.ThrowIfCancellationRequested();

            // Get the first file from the directory
            var fileToRoute = Directory.EnumerateFiles(_routingConnection.Folders.FilesConnectionInQueue, "*.eml").FirstOrDefault() ??
                              throw new UnableToRetrieveMessageToSendException($"The folder \"{_routingConnection.Folders.FilesConnectionInQueue}\" is empty");

            // Move it to the Routing Folder
            var fileToRouteInfo = new FileInfo(fileToRoute);
            MultiAttemptHelper.FileMove(fileToRoute, 
                                        Path.Combine(_routingConnection.Folders.FilesConnectionSending, fileToRouteInfo.Name));

            return fileToRouteInfo.Name;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (UnableToRetrieveMessageToSendException)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError(LoggingEvents.UnableToRetrieveMessagesToSend, e, $"Error on {nameof(GetNextFileNameAsync)} function");
            throw;
        }
        finally
        {
            _folderLockSemaphore.Release();
        }
    }

    /// <summary>
    /// Sends the message thru the destination SMTP
    /// </summary>
    /// <remarks>Up to 10 attemps are made, based on the connection configuration</remarks>
    /// <param name="client">The reusable client</param>
    /// <param name="fileToSendNameOnly">Name of the file to be send (without the full path)</param>
    /// <param name="message">The message to be sent</param>
    /// <returns></returns>
    /// <exception cref="UnableToSendMessageException">Thrown when the system could not send the email after the maximum attempts</exception>
    private async Task SendMessageAsync(SmtpClient client, string fileToSendNameOnly, SmtpMessage message)
    {
        var currentAttempt = 0;

        while (currentAttempt <= _routingConnection.MaximumRetryAttempts)
        {
            // Increment Count
            currentAttempt++;

            try
            {
                // Loads the message contents
                using var mimeMessage = MimeKit.MimeMessage.Load(message.GetContentsAsStream());

                var recipients = new List<MailboxAddress>();
                foreach (var mTo in message.Recipients)
                    recipients.Add(new MailboxAddress(mTo.ToString(), mTo?.ToString()));

                // Connects to the SMTP
                if (!client.IsConnected)
                {
                    await client.ConnectAsync(_routingConnection.ConnectionInfo.Host, _routingConnection.ConnectionInfo.Port, (MailKit.Security.SecureSocketOptions)_routingConnection.ConnectionInfo.SecureSocketOption);
                }

                // Sends the message thru the final SMTP
                await client.SendAsync(mimeMessage,
                                       new MailboxAddress(message.MailFrom?.ToString(), message.MailFrom?.ToString()), 
                                       recipients);

                // Move file to Sent folder (based on the grouping options)
                try
                {
                    MultiAttemptHelper.FileMove(Path.Combine(_routingConnection.Folders.FilesConnectionSending, fileToSendNameOnly),
                                                Path.Combine(_routingConnection.Folders.GetSentFolderWithGrouping(_routingConnection.ConnectionInfo.GroupingOption, DateTime.Now), fileToSendNameOnly));
                }
                catch (Exception e)
                {
                    // Log the attempt
                    _logger?.LogError(LoggingEvents.FileIOError, e, "Unable to move message \"{fileToSend}\" to the proper sent folder", fileToSendNameOnly);
                }

                // Exit the loop, since no errors happened
                break;
            }
            catch (Exception e)
            {
                // When it reaches the limit, throws the exception and leaves the function
                if (currentAttempt >= _routingConnection.MaximumRetryAttempts)
                {
                    // Log the attempt
                    _logger?.LogError(LoggingEvents.UnableToSendMessage, e, "Unable to send message \"{fileToSend}\". Attempt {attempt} of {maxAttemps}", fileToSendNameOnly, currentAttempt, _routingConnection.MaximumRetryAttempts);

                    throw new UnableToSendMessageException(message, fileToSendNameOnly, e);
                }
            }
        }
    }
}
