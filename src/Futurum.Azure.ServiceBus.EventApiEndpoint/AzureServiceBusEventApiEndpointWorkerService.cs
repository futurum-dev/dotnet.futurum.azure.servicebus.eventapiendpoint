using Azure.Messaging.ServiceBus;

using Futurum.Azure.ServiceBus.EventApiEndpoint.Metadata;
using Futurum.Core.Result;
using Futurum.EventApiEndpoint;
using Futurum.EventApiEndpoint.Metadata;
using Futurum.Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Futurum.Azure.ServiceBus.EventApiEndpoint;

public interface IAzureServiceBusEventApiEndpointWorkerService
{
    Task ExecuteAsync(CancellationToken stoppingToken);

    Task StopAsync(CancellationToken cancellationToken);
}

public class AzureServiceBusEventApiEndpointWorkerService : IAzureServiceBusEventApiEndpointWorkerService
{
    private readonly IEventApiEndpointLogger _logger;
    private readonly AzureServiceBusConnectionConfiguration _connectionConfiguration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventApiEndpointMetadataCache _metadataCache;

    private ServiceBusClient _serviceBusClient;

    private readonly List<ServiceBusProcessor> _serviceBusProcessors = new();

    public AzureServiceBusEventApiEndpointWorkerService(IEventApiEndpointLogger logger,
                                                        AzureServiceBusConnectionConfiguration connectionConfiguration,
                                                        IServiceProvider serviceProvider,
                                                        IEventApiEndpointMetadataCache metadataCache)
    {
        _logger = logger;
        _connectionConfiguration = connectionConfiguration;
        _serviceProvider = serviceProvider;
        _metadataCache = metadataCache;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _serviceBusClient = new ServiceBusClient(_connectionConfiguration.ConnectionString);

        await ConfigureCommands();

        await ConfigureBatchCommands();

        await StartAll(stoppingToken);
    }

    private async Task ConfigureCommands()
    {
        var metadataDefinitions = _metadataCache.GetMetadataEventDefinitions();

        foreach (var (metadataSubscriptionDefinition, metadataTypeDefinition) in metadataDefinitions)
        {
            if (metadataSubscriptionDefinition is MetadataSubscriptionEventDefinition azureServiceBusMetadataSubscriptionEventDefinition)
            {
                await ConfigureSubscription(azureServiceBusMetadataSubscriptionEventDefinition, metadataTypeDefinition.EventApiEndpointExecutorServiceType);
            }
        }
    }

    private async Task ConfigureBatchCommands()
    {
        var metadataEnvelopeCommandDefinitions = _metadataCache.GetMetadataEnvelopeEventDefinitions();

        var metadataSubscriptionCommandDefinitions = metadataEnvelopeCommandDefinitions.Select(x => x.MetadataSubscriptionEventDefinition)
                                                                                       .Select(x => x.FromTopic)
                                                                                       .Distinct()
                                                                                       .Select(topic => new MetadataSubscriptionEventDefinition(topic));

        foreach (var envelopeMetadataSubscriptionCommandDefinition in metadataSubscriptionCommandDefinitions)
        {
            var apiEndpointExecutorServiceType = typeof(EventApiEndpointExecutorService<,>).MakeGenericType(typeof(Batch.EventDto), typeof(Batch.Event));

            await ConfigureSubscription(envelopeMetadataSubscriptionCommandDefinition, apiEndpointExecutorServiceType);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopAll(cancellationToken);
    }

    private async Task ConfigureSubscription(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, Type apiEndpointExecutorServiceType)
    {
        try
        {
            var serviceBusProcessor = _serviceBusClient.CreateProcessor(metadataSubscriptionDefinition.Queue.Value, new ServiceBusProcessorOptions());
            _serviceBusProcessors.Add(serviceBusProcessor);

            // add handler to process messages
            serviceBusProcessor.ProcessMessageAsync += processMessageEventArgs => OnProcessMessageAsync(metadataSubscriptionDefinition, apiEndpointExecutorServiceType, processMessageEventArgs);

            // add handler to process any errors
            serviceBusProcessor.ProcessErrorAsync += processMessageEventArgs => OnProcessErrorAsync(metadataSubscriptionDefinition, processMessageEventArgs);

            // start processing 
            await serviceBusProcessor.StartProcessingAsync();
        }
        catch (Exception exception)
        {
            _logger.EventProcessorClientConfigurationError(metadataSubscriptionDefinition, exception);
        }
    }

    private async Task OnProcessMessageAsync(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, Type apiEndpointExecutorServiceType, ProcessMessageEventArgs processMessageEventArgs)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var message = processMessageEventArgs.Message.Body.ToString();

        await scope.ServiceProvider.TryGetService<IEventApiEndpointExecutorService>(apiEndpointExecutorServiceType)
                   .ThenAsync(apiEndpointExecutorService => apiEndpointExecutorService.ExecuteAsync(metadataSubscriptionDefinition, message, processMessageEventArgs.CancellationToken))
                   .DoAsync(_ => processMessageEventArgs.CompleteMessageAsync(processMessageEventArgs.Message))
                   .DoWhenFailureAsync(error => _logger.ServiceBusProcessorProcessEventError(metadataSubscriptionDefinition, error));
    }

    private Task OnProcessErrorAsync(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, ProcessErrorEventArgs processMessageEventArgs)
    {
        _logger.ServiceBusProcessorProcessError(metadataSubscriptionDefinition, processMessageEventArgs.ErrorSource, processMessageEventArgs.EntityPath,
                                                processMessageEventArgs.FullyQualifiedNamespace, processMessageEventArgs.Exception);

        return Task.CompletedTask;
    }

    private async Task StartAll(CancellationToken stoppingToken)
    {
        foreach (var serviceBusProcessor in _serviceBusProcessors)
        {
            try
            {
                await serviceBusProcessor.StartProcessingAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.ServiceBusProcessorStartProcessingError(exception);
            }
        }
    }

    private async Task StopAll(CancellationToken cancellationToken)
    {
        foreach (var serviceBusProcessor in _serviceBusProcessors)
        {
            try
            {
                await serviceBusProcessor.StopProcessingAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.ServiceBusProcessorStopProcessingError(exception);
            }
        }
    }
}