namespace Futurum.Azure.ServiceBus.EventApiEndpoint.Sample;

public class Worker : BackgroundService
{
    private readonly IAzureServiceBusEventApiEndpointWorkerService _workerService;

    public Worker(IAzureServiceBusEventApiEndpointWorkerService workerService)
    {
        _workerService = workerService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        _workerService.ExecuteAsync(stoppingToken);

    public override Task StopAsync(CancellationToken cancellationToken) =>
        _workerService.StopAsync(cancellationToken);
}