#pragma warning disable IDE0063 // Use simple 'using' statement

using SMTPRouter.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace SMTPRouter.Core;

/// <summary>
/// Represents a Message Received thru SMTP
/// </summary>
public sealed class SmtpMessage: IMessage
{
    // Constants for the SmtpRouter Header Tag
    internal const string SMTPROUTER_HEADER = "SmtpRouter-Header";
    internal const string SMTPROUTER_HEADER_BEGIN = "SmtpRouter-Header-Begin";
    internal const string SMTPROUTER_HEADER_VERSION = "SmtpRouter-Header-Version";
    internal const string SMTPROUTER_HEADER_FROM = "SmtpRouter-Header-From";
    internal const string SMTPROUTER_HEADER_TO = "SmtpRouter-Header-To";
    internal const string SMTPROUTER_HEADER_END = "SmtpRouter-Header-End";
    internal const string SMTPROUTER_HEADER_CREATIONTIME = "SmtpRouter-Header-CreationTime";
    internal const string SMTPROUTER_HEADER_ORIGIN_IP_ADDRESS = "SmtpRouter-Header-OriginIPAddress";
    internal const string SMTPROUTER_HEADER_RECEIVEDBY_IP_ADDRESS = "SmtpRouter-Header-ReceivedByIPAddress";
    internal const string SMTPROUTER_HEADER_RECEIVEDBY_HOSTNAME = "SmtpRouter-Header-ReceivedByHostname";
    internal const string SMTPROUTER_HEADER_FORCEROUTING = "SmtpRouter-Header-ForceRouting";
    internal const string SMTPROUTER_VERSION = "3.0.0.0";

    /// <summary>
    /// The default format to use when serializing and/or deserializing messages from streams
    /// </summary>
    public const string SMTPROUTER_HEADER_CREATIONTIME_FORMAT = "yyyy-MM-dd_HH-mm-ss";

    /// <summary>
    /// The Message Unique Identifier
    /// </summary>
    public string? ID { get; set; }

    /// <summary>
    /// The DateTime stamp when the message was first created
    /// </summary>
    public DateTime CreationDateTime { get; set; }

    /// <inheritdoc/>
    public string? Contents { get; set; }

    /// <summary>
    /// A flag to define whether the message will be routed even though the acceptance/rejection rules do not allow that
    /// </summary>
    public bool ForceRouting { get; set; }

    /// <inheritdoc/>
    public string? OriginIPAddress { get; set; }

    /// <inheritdoc/>
    public string? Sender { get; set; }

    /// <summary>
    /// The IP Address that received the message
    /// </summary>
    public string? ReceivedByIPAddress { get; set; }

    /// <summary>
    /// The name of the machine that received the message
    /// </summary>
    public string? ReceivedByHostName { get; set; }

    /// <summary>
    /// The mailbox sending the message
    /// </summary>
    public SmtpMailbox? MailFrom { get; set; }

    /// <summary>
    /// List of recipients of the message
    /// </summary>
    public List<SmtpMailbox> Recipients { get; set; } = new();

    /// <summary>
    /// A list of parameters sent with the message
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();

    private static readonly object _internalCounterLock = new();

    /// <summary>
    /// An internal numeric counter to attempt to give a unique number for each file
    /// </summary>
    /// <remarks>Using a <see cref="uint"/> type to give a maximum of 4,294,967,295 files at the same second</remarks>
    private static uint _internalCounter = 0;

    /// <summary>
    /// Returns the next File Id
    /// </summary>
    /// <remarks>Once it reaches the end, it will automatically restart</remarks>
    /// <returns>The Next File Id</returns>
    public static uint GetNextFileId()
    {
        lock (_internalCounterLock)
        {
            _internalCounter++;
        }
        
        return _internalCounter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpMessage"/> class
    /// </summary>
    public SmtpMessage()
    {
        CreationDateTime = DateTime.Now;
    }

    /// <summary>
    /// Creates an instance of an <see cref="SmtpMessage"/> based on a file
    /// </summary>
    /// <param name="filename">The file name</param>
    /// <returns>An <see cref="SmtpMessage"/> based on the <paramref name="filename"/></returns>
    public static SmtpMessage LoadFile(string filename)
    {
        var message = new SmtpMessage();
        var fileLocation = FileLocation.Undefined;

        // Opens the file
        using (var fileStream = MultiAttemptHelper.FileOpenRead(filename))
        {
            string? line = "";

            if (fileStream is null) throw new NullReferenceException($"The File Stream for \"{filename}\" could not be created");

            // Read each each line of the file and attemps to make sense of it
            using (var reader = new StreamReader(fileStream, Encoding.UTF8)) 
            {
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith(SMTPROUTER_HEADER_BEGIN))
                    {
                        fileLocation = FileLocation.Header;
                        continue;
                    }
                    else if (line.StartsWith(SMTPROUTER_HEADER_END))
                    {
                        fileLocation = FileLocation.Contents;
                    }

                    if (fileLocation == FileLocation.Header)
                    {
                        var headerData = line.Split(':');

                        if (headerData.Length != 2) continue;
                        headerData[1] = headerData[1].Trim();

                        switch (headerData[0])
                        {
                            case SMTPROUTER_HEADER_CREATIONTIME:
                                message.CreationDateTime = DateTime.ParseExact(headerData[1], 
                                                                               SMTPROUTER_HEADER_CREATIONTIME_FORMAT, 
                                                                               System.Globalization.CultureInfo.InvariantCulture);
                                break;

                            case SMTPROUTER_HEADER_FROM:
                                message.Sender = headerData[1];
                                message.MailFrom = new SmtpMailbox(headerData[1]);
                                break;

                            case SMTPROUTER_HEADER_ORIGIN_IP_ADDRESS:
                                message.OriginIPAddress = headerData[1];
                                break;

                            case SMTPROUTER_HEADER_RECEIVEDBY_IP_ADDRESS:
                                message.ReceivedByIPAddress = headerData[1];
                                break;

                            case SMTPROUTER_HEADER_RECEIVEDBY_HOSTNAME:
                                message.ReceivedByHostName = headerData[1];
                                break;

                            case SMTPROUTER_HEADER_TO:
                                var mailTo = new SmtpMailbox(headerData[1]);
                                if (!message.Recipients.Contains(mailTo))
                                    message.Recipients.Add(new SmtpMailbox(headerData[1]));

                                break;
                        }

                        continue;
                    }

                    if (fileLocation == FileLocation.Contents)
                    {
                        message.Contents = reader.ReadToEnd();
                        continue;
                    }
                }
            }
        }

        return message;
    }

    /// <summary>
    /// Save the <see cref="SmtpMessage"/> into a text file
    /// </summary>
    /// <param name="path">The path where the file should be saved</param>
    /// <returns>The full path of the file that was just generated</returns>
    public string SaveToFile(string path)
    {
        // Minimum Parameters needed
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        if (string.IsNullOrWhiteSpace(Contents)) throw new NullReferenceException(nameof(Contents));

        // Define Output File Name
        string fileName = Path.Combine(path, $"{CreationDateTime:yyyyMMddHHmmss}-{GetNextFileId():0000000000}.eml");

        // Create Output File
        using (var fileStream = File.Create(fileName))
        {
            using var stream = new MemoryStream();
            using var streamWriter = new StreamWriter(stream, Encoding.UTF8);

            streamWriter.AutoFlush = false;

            streamWriter.WriteLine(SMTPROUTER_HEADER_BEGIN);
            streamWriter.WriteLine($"{SMTPROUTER_HEADER_VERSION}: {SMTPROUTER_VERSION}");
            streamWriter.WriteLine($"{SMTPROUTER_HEADER_CREATIONTIME}: {CreationDateTime.ToString(SMTPROUTER_HEADER_CREATIONTIME_FORMAT)}");
            streamWriter.WriteLine($"{SMTPROUTER_HEADER_FROM}: {MailFrom}");
            streamWriter.WriteLine($"{SMTPROUTER_HEADER_ORIGIN_IP_ADDRESS}: {OriginIPAddress}");
            streamWriter.WriteLine($"{SMTPROUTER_HEADER_RECEIVEDBY_IP_ADDRESS}: {ReceivedByIPAddress}");
            streamWriter.WriteLine($"{SMTPROUTER_HEADER_RECEIVEDBY_HOSTNAME}: {ReceivedByHostName}");

            foreach (var mailTo in Recipients)
                streamWriter.WriteLine($"{SMTPROUTER_HEADER_TO}: {mailTo}");

            streamWriter.WriteLine(SMTPROUTER_HEADER_END);
            streamWriter.WriteLine(Contents);

            streamWriter.Flush();

            // Copy To File
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);

            fileStream.Flush();
        }

        return fileName;
    }

    /// <summary>
    /// Converts the email contents into a stream 
    /// </summary>
    /// <returns>A Stream containig the message contents in UTF8</returns>
    public Stream GetContentsAsStream()
    {
        var bytes = Encoding.UTF8.GetBytes(Contents is null ? string.Empty : Contents);
        return new MemoryStream(bytes);
    }
}

enum FileLocation: byte
{
    Undefined = 0,
    Header = 1,
    Contents = 2
}

#pragma warning restore IDE0063 // Use simple 'using' statement
