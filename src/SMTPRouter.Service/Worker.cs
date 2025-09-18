using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMTPRouter;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SMTPRouter
{
    /// <summary>
    /// 
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Execution Started: {time}", DateTimeOffset.Now);

                // Read Configuration
                var setup = System.Text.Json.JsonSerializer.Deserialize<Setup>(System.IO.File.ReadAllText("setup.json"));

                if (setup is null)
                    throw new Exception("The Configuration File could not be loaded");

                if (setup.Hosting is null)
                    throw new Exception("The \"Hosting\" section in the Configuration File is not properly setup");

                if (setup.Hosting.PortsConfiguration is null)
                    throw new Exception("The \"Hosting.PortsConfiguration\" section in the Configuration File is not properly setup");

                if (setup.Hosting.PortsConfiguration.Count == 0)
                    throw new Exception("The \"Hosting.PortsConfiguration\" section in the Configuration File does not contain any port");

                if (setup.Connections is null)
                    throw new Exception("The \"Connections\" section in the Configuration File is not properly setup");



                //TODO: Add the Rules Section

                // Create the SMTP Server Listener





                while (!stoppingToken.IsCancellationRequested)
                {
                    

                    await Task.Delay(1000, stoppingToken);
                }

            }
            catch (OperationCanceledException)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Operation Cancelled at: {time}", DateTimeOffset.Now);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occurred in the method \"{nameof(ExecuteAsync)}\" of the \"{nameof(Worker)}\" class");
            }
            finally
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Execution Finished: {time}", DateTimeOffset.Now);
            }
        }
    }
}
