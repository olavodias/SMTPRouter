using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace SMTPRouter;

/// <summary>
/// Represents a Message Received thru SMTP
/// </summary>
public sealed class SmtpMessage
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

    /// <summary>
    /// The Contents of the Message
    /// </summary>
    public string? Contents { get; set; }

    /// <summary>
    /// A flag to define whether the message will be routed even though the acceptance/rejection rules do not allow that
    /// </summary>
    public bool ForceRouting { get; set; }

    /// <summary>
    /// The IP Address sending the message
    /// </summary>
    public string? OriginIPAddress { get; set; }

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

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpMessage"/> class
    /// </summary>
    public SmtpMessage()
    {
        CreationDateTime = DateTime.Now;
    }

    /// <summary>
    /// Save the <see cref="SmtpMessage"/> into a text file
    /// </summary>
    /// <param name="path">The path where the file should be saved</param>
    /// <returns>The full path of the file that was just generated</returns>
    public string SaveToFile(string path)
    {
        try
        {
            // Minimum Parameters needed
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(Contents)) throw new ArgumentNullException(nameof(Contents));

            // Define Output File Name
            var i = 1;
            string fileName = Path.Combine(path, $"{CreationDateTime:yyyyMMddHHmmss}-{i:3}.eml");
            while (File.Exists(fileName))
            {
                i++;
                fileName = Path.Combine(path, $"{CreationDateTime:yyyyMMddHHmmss}-{i:3}.eml");
            }

            // Create Output File
            using (var fileStream = File.Create(fileName))
            {
                using var stream = new MemoryStream();
                using var streamWriter = new StreamWriter(stream, Encoding.GetEncoding(28592));

                streamWriter.AutoFlush = false;

                streamWriter.WriteLine(SMTPROUTER_HEADER_BEGIN);
                streamWriter.WriteLine($"{SMTPROUTER_HEADER_VERSION}: {SMTPROUTER_VERSION}");
                streamWriter.WriteLine($"{SMTPROUTER_HEADER_CREATIONTIME}: {CreationDateTime.ToString(SMTPROUTER_HEADER_CREATIONTIME_FORMAT)}");
                streamWriter.WriteLine($"{SMTPROUTER_HEADER_FROM}: {MailFrom}");
                streamWriter.WriteLine($"{SMTPROUTER_HEADER_ORIGIN_IP_ADDRESS}: {OriginIPAddress}");

                if (Recipients is not null)
                {
                    foreach (var mailTo in Recipients)
                        streamWriter.WriteLine($"{SMTPROUTER_HEADER_TO}: {mailTo}");
                }

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
        catch (Exception)
        {

            throw;
        }
    }
}
