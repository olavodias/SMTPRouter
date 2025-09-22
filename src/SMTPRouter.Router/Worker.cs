namespace SMTPRouter.Router;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RouterProcessor _routerProcessor;

    public Worker(ILogger<Worker> logger, RouterProcessor routerProcessor)
    {
        _logger = logger;
        _routerProcessor = routerProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var taskList = new List<Task>
        {
            _routerProcessor.DoWorkAsync(stoppingToken)
        };

        await Task.WhenAll(taskList);
    }
}
