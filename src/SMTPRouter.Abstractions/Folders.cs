using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMTPRouter;

/// <summary>
/// Stores the list of folders used by the framework
/// </summary>
/// <remarks>Folder naming should follow certain rules. The last digit of the folder number has the following meaning:
/// <list type="bullet">
/// <item>1 => Message is Waiting For Processing</item>
/// <item>2 => Message is In Process</item>
/// <item>5 => Step had errors</item>
/// <item>9 => Final Status of the Step</item>
/// <item>Number 3, 4, 6, 7, and 8 can be used for additional in-between steps</item>
/// </list>
/// </remarks>
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
    public const string FILES_LISTENER_REJECTED = "08-Rejected";
    /// <summary>
    /// The folder where the messages accepted by the Listener are stored
    /// </summary>
    public const string FILES_LISTENER_RECEIVED = "09-Received";

    /// <summary>
    /// The folder where the messages that were accepted but still failed to process are stored
    /// </summary>
    public const string FILES_LISTENER_ERRORS = "05-Errors";


    /// <summary>
    /// The folder where the messages being routed are stored
    /// </summary>
    public const string FILES_ROUTER_ROUTING = "12-Routing";
    /// <summary>
    /// The folder where the messages successfully routed are stored
    /// </summary>
    /// <remarks>Each SMTP Key has its own sub-folder underneath the routed folder</remarks>
    public const string FILES_ROUTER_ROUTED = "19-Routed";
    /// <summary>
    /// The folder where the messages that could not be routed are stored
    /// </summary>
    public const string FILES_ROUTER_ERROR = "15-Errors";


    /// <summary>
    /// The folder where the routed messages waiting to be sent are stored
    /// </summary>
    public const string FILES_CONNECTION_IN_QUEUE = "31-InQueue";
    /// <summary>
    /// The folder where the routed messages being sent are stored
    /// </summary>
    public const string FILES_CONNECTION_SENDING = "32-Sending";
    /// <summary>
    /// The folder where the routed messages that were successfully processed are stored
    /// </summary>
    /// <remarks>The files are stored based on the Grouping Option parameter in the connection configuration</remarks>
    public const string FILES_CONNECTION_SENT = "39-Sent";
    /// <summary>
    /// The folder where the routed messages that could not be sent are stored
    /// </summary>
    public const string FILES_CONNECTION_ERRORS = "35-Errors";

    /// <summary>
    /// Initializes a new instance of the <see cref="Folders"/> class
    /// </summary>
    /// <param name="rootPath">The hosting root path</param>
    public Folders(string rootPath)
    {
        RootPath = rootPath;
        Files = Path.Combine(rootPath, FILES);
        
        FilesListenerRejected = Path.Combine(rootPath, FILES, FILES_LISTENER_REJECTED);
        FilesListenerReceived = Path.Combine(rootPath, FILES, FILES_LISTENER_RECEIVED);
        FilesListenerErrors = Path.Combine(rootPath, FILES, FILES_LISTENER_ERRORS);

        FilesRouterRouting = Path.Combine(rootPath, FILES, FILES_ROUTER_ROUTING);
        FilesRouterRouted = Path.Combine(rootPath, FILES, FILES_ROUTER_ROUTED);
        FilesRouterError = Path.Combine(rootPath, FILES, FILES_ROUTER_ERROR);

        if (!Directory.Exists(FilesListenerRejected)) Directory.CreateDirectory(FilesListenerRejected);
        if (!Directory.Exists(FilesListenerReceived)) Directory.CreateDirectory(FilesListenerReceived);
        if (!Directory.Exists(FilesListenerErrors)) Directory.CreateDirectory(FilesListenerErrors);

        if (!Directory.Exists(FilesRouterRouting)) Directory.CreateDirectory(FilesRouterRouting);
        if (!Directory.Exists(FilesRouterRouted)) Directory.CreateDirectory(FilesRouterRouted);
        if (!Directory.Exists(FilesRouterError)) Directory.CreateDirectory(FilesRouterError);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Folders"/> class
    /// </summary>
    /// <param name="rootPath">The hosting root path</param>
    /// <param name="connectionName">The name of the connection</param>
    public Folders(string rootPath, string connectionName): this(rootPath)
    {
        FilesConnectionErrors = Path.Combine(FilesRouterRouted, connectionName, FILES_CONNECTION_ERRORS);
        FilesConnectionInQueue = Path.Combine(FilesRouterRouted, connectionName, FILES_CONNECTION_IN_QUEUE);
        FilesConnectionSending = Path.Combine(FilesRouterRouted, connectionName, FILES_CONNECTION_SENDING);
        FilesConnectionSent = Path.Combine(FilesRouterRouted, connectionName, FILES_CONNECTION_SENT);

        if (!Directory.Exists(FilesConnectionErrors)) Directory.CreateDirectory(FilesConnectionErrors);
        if (!Directory.Exists(FilesConnectionInQueue)) Directory.CreateDirectory(FilesConnectionInQueue);
        if (!Directory.Exists(FilesConnectionSending)) Directory.CreateDirectory(FilesConnectionSending);
        if (!Directory.Exists(FilesConnectionSent)) Directory.CreateDirectory(FilesConnectionSent);
    }

    /// <summary>
    /// The root for the folder structure
    /// </summary>
    public string RootPath { get; }

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



    /// <summary>
    /// The folder where the routed messages waiting to be sent are stored
    /// </summary>
    /// <remarks>This folder is not defined unless a connection name is specified in the constructor</remarks>
    public string FilesConnectionInQueue { get; } = string.Empty;
    /// <summary>
    /// The folder where the routed messages being sent are stored
    /// </summary>
    /// <remarks>This folder is not defined unless a connection name is specified in the constructor</remarks>
    public string FilesConnectionSending { get; } = string.Empty;
    /// <summary>
    /// The folder where the routed messages that were successfully processed are stored
    /// </summary>
    /// <remarks>The files are stored based on the Grouping Option parameter in the connection configuration</remarks>
    /// <remarks>This folder is not defined unless a connection name is specified in the constructor</remarks>
    public string FilesConnectionSent { get; } = string.Empty;
    /// <summary>
    /// The folder where the routed messages that could not be sent are stored
    /// </summary>
    /// <remarks>This folder is not defined unless a connection name is specified in the constructor</remarks>
    public string FilesConnectionErrors { get; } = string.Empty;

}
