using System;

namespace SMTPRouter;

/// <summary>
/// Event Arguments for the event when a <see cref="SmtpMessage"/> is received
/// </summary>
public class MessageEventArgs: EventArgs
{
    /// <summary>
    /// The message received
    /// </summary>
    public SmtpMessage? SmtpMessage { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the Message Received Event Arguments
    /// </summary>
    public MessageEventArgs(): this(null)
    {

    }

    /// <summary>
    /// Initializes a new instance of the Message Received Event Arguments
    /// </summary>
    /// <param name="smtpMessage">The <see cref="SmtpMessage"/> received by the Smtp</param>
    public MessageEventArgs(SmtpMessage? smtpMessage) 
    {
        SmtpMessage = smtpMessage;
    }

}
