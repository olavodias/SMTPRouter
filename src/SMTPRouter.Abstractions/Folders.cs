using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMTPRouter;

/// <summary>
/// Stores the list of folders used by the framework
/// </summary>
public sealed class Folders
{
    /// <summary>
    /// The folder where the files are located
    /// </summary>
    public const string FILES = "Files";

    /// <summary>
    /// The folder where the messages rejected by the Listener are stored
    /// </summary>
    /// <remarks>Depending on how the system is configured, rejected messages may not be stored.</remarks>
    public const string FILES_LISTENER_REJECTED = "09-Rejected";
    /// <summary>
    /// The folder where the messages accepted by the Listener are stored
    /// </summary>
    public const string FILES_LISTENER_RECEIVED = "10-Received";
    /// <summary>
    /// The folder where the messages that were accepted but still failed to process are stored
    /// </summary>
    public const string FILES_LISTENER_ERRORS = "10E-Errors";


    /// <summary>
    /// The folder where the messages being routed are stored
    /// </summary>
    public const string FILES_ROUTER_ROUTING = "15-Routing";
    /// <summary>
    /// The folder where the messages successfully routed are stored
    /// </summary>
    /// <remarks>Each SMTP Key has its own sub-folder underneath the routed folder</remarks>
    public const string FILES_ROUTER_ROUTED = "20-Routed";
    /// <summary>
    /// The folder where the messages that could not be routed are stored
    /// </summary>
    public const string FILES_ROUTER_ERROR = "20E-Errors";


    /// <summary>
    /// The folder where the routed messages waiting to be sent are stored
    /// </summary>
    public const string FILES_CONNECTION_IN_QUEUE = "30-InQueue";
    /// <summary>
    /// The folder where the routed messages being sent are stored
    /// </summary>
    public const string FILES_CONNECTION_SENDING = "35-Sending";
    /// <summary>
    /// The folder where the routed messages that were successfully processed are stored
    /// </summary>
    /// <remarks>The files are stored based on the Grouping Option parameter in the connection configuration</remarks>
    public const string FILES_CONNECTION_SENT = "40-Sent";
    /// <summary>
    /// The folder where the routed messages that could not be sent are stored
    /// </summary>
    public const string FILES_CONNECTION_ERRORS = "40E-Sent";


    /// <summary>
    /// Initializes a new instance of the <see cref="Folders"/> class
    /// </summary>
    /// <param name="rootPath">The hosting root path</param>
    public Folders(string rootPath)
    {
        Files = Path.Combine(rootPath, FILES);
        
        FilesListenerRejected = Path.Combine(rootPath, FILES, FILES_LISTENER_REJECTED);
        FilesListenerReceived = Path.Combine(rootPath, FILES, FILES_LISTENER_RECEIVED);
        FilesListenerErrors = Path.Combine(rootPath, FILES, FILES_LISTENER_ERRORS);

        FilesRouterRouting = Path.Combine(rootPath, FILES, FILES_ROUTER_ROUTING);
        FilesRouterRouted = Path.Combine(rootPath, FILES, FILES_ROUTER_ROUTED);
        FilesRouterError = Path.Combine(rootPath, FILES, FILES_ROUTER_ERROR);
    }

    /// <summary>
    /// The folder where the files are located
    /// </summary>
    public string Files { get; }

    /// <summary>
    /// The folder where the messages rejected by the Listener are stored
    /// </summary>
    /// <remarks>Depending on how the system is configured, rejected messages may not be stored.</remarks>
    public string FilesListenerRejected { get; }
    /// <summary>
    /// The folder where the messages accepted by the Listener are stored
    /// </summary>
    public string FilesListenerReceived { get; }
    /// <summary>
    /// The folder where the messages that were accepted but still failed to process are stored
    /// </summary>
    public string FilesListenerErrors { get; }


    /// <summary>
    /// The folder where the messages being routed are stored
    /// </summary>
    public string FilesRouterRouting { get; }
    /// <summary>
    /// The folder where the messages successfully routed are stored
    /// </summary>
    /// <remarks>Each SMTP Key has its own sub-folder underneath the routed folder</remarks>
    public string FilesRouterRouted { get; }
    /// <summary>
    /// The folder where the messages that could not be routed are stored
    /// </summary>
    public string FilesRouterError { get; }

}
