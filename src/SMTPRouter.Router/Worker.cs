namespace SMTPRouter.Router;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RouterProcessor _routerProcessor;
    private readonly ConnectionOrchestrationProcessor _orchestrationProcessor;

    public Worker(ILogger<Worker> logger, RouterProcessor routerProcessor, ConnectionOrchestrationProcessor orchestrationProcessor)
    {
        _logger = logger;
        _routerProcessor = routerProcessor;
        _orchestrationProcessor = orchestrationProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var taskList = new List<Task>
        {
            _routerProcessor.DoWorkAsync(stoppingToken),
            _orchestrationProcessor.DoWorkAsync(stoppingToken)
        };

        await Task.WhenAll(taskList);
    }
}
