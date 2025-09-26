using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.ComponentModel;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter.Listener
{
    /// <summary>
    /// The Worker that will listen to SMTP Messages and save them
    /// </summary>
    internal class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IProcessor _processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="processor">The process to run</param>
        public Worker(ILogger<Worker> logger, IProcessor processor)
        {
            _logger = logger;
            _processor = processor;
        }
        
        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _processor.DoWorkAsync(stoppingToken);

                //TODO: Implement Purge Process

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
