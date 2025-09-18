using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SMTPRouter.Listener;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<ListenerWorker>();

var host = builder.Build();
host.Run();
