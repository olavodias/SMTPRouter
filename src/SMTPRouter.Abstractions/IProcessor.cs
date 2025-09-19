using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter;

/// <summary>
/// Defines the Interface to be implemented by processes
/// </summary>
/// <remarks>Use it for the Background Services, to make them more unit testable</remarks>
internal interface IProcessor
{
    /// <summary>
    /// Perform the Asynchronous Wort
    /// </summary>
    /// <param name="stoppingToken">The cancellation token to stop the process</param>
    /// <returns></returns>
    Task DoWorkAsync(CancellationToken stoppingToken);

}
