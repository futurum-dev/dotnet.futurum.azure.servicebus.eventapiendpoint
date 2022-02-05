using Azure.Messaging.ServiceBus;

using Futurum.Azure.ServiceBus.EventApiEndpoint.Metadata;
using Futurum.Core.Result;

using Serilog;

namespace Futurum.Azure.ServiceBus.EventApiEndpoint;

public interface IEventApiEndpointLogger : Futurum.EventApiEndpoint.IEventApiEndpointLogger, ApiEndpoint.IApiEndpointLogger
{
    void EventProcessorClientConfigurationError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, Exception exception);

    void ServiceBusProcessorProcessError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, ServiceBusErrorSource errorSource, string entityPath,
                                         string fullyQualifiedNamespace, Exception exception);

    void ServiceBusProcessorProcessEventError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, IResultError error);

    void ServiceBusProcessorStartProcessingError(Exception exception);

    void ServiceBusProcessorStopProcessingError(Exception exception);
}

public class EventApiEndpointLogger : IEventApiEndpointLogger
{
    private readonly ILogger _logger;

    public EventApiEndpointLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void EventReceived<TEvent>(TEvent @event)
    {
        var eventData = new EventReceivedData<TEvent>(typeof(TEvent), @event);

        _logger.Debug("AzureServiceBus EventApiEndpoint event received {@eventData}", eventData);
    }

    public void EventProcessorClientConfigurationError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, Exception exception)
    {
        var eventData = new ConfigurationErrorData(metadataSubscriptionDefinition);

        _logger.Error(exception, "AzureServiceBus ServiceBusProcessor Configuration error {@eventData}", eventData);
    }

    public void ServiceBusProcessorProcessError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, ServiceBusErrorSource errorSource, string entityPath,
                                                string fullyQualifiedNamespace, Exception exception)
    {
        var eventData = new ProcessErrorData(metadataSubscriptionDefinition, errorSource, entityPath, fullyQualifiedNamespace);

        _logger.Error(exception, "AzureServiceBus ServiceBusProcessor ProcessError error {@eventData}", eventData);
    }

    public void ServiceBusProcessorProcessEventError(MetadataSubscriptionEventDefinition metadataSubscriptionDefinition, IResultError error)
    {
        var eventData = new ProcessEventErrorData(metadataSubscriptionDefinition, error.ToErrorString());

        _logger.Error("AzureServiceBus ServiceBusProcessor ProcessEventError error {@eventData}", eventData);
    }

    public void ServiceBusProcessorStartProcessingError(Exception exception)
    {
        _logger.Error(exception, "AzureServiceBus ServiceBusProcessor StartProcessing error");
    }

    public void ServiceBusProcessorStopProcessingError(Exception exception)
    {
        _logger.Error(exception, "AzureServiceBus ServiceBusProcessor StopProcessing error");
    }

    public void ApiEndpointDebugLog(string apiEndpointDebugLog)
    {
        var eventData = new ApiEndpoints(apiEndpointDebugLog);

        _logger.Debug("WebApiEndpoint endpoints {@eventData}", eventData);
    }

    private readonly record struct EventReceivedData<TEvent>(Type EventType, TEvent Event);

    private readonly record struct ConfigurationErrorData(MetadataSubscriptionEventDefinition MetadataSubscriptionDefinition);

    private readonly record struct ProcessErrorData(MetadataSubscriptionEventDefinition MetadataSubscriptionDefinition, ServiceBusErrorSource ErrorSource, string EntityPath,
                                                    string FullyQualifiedNamespace);

    private readonly record struct ProcessEventErrorData(MetadataSubscriptionEventDefinition MetadataSubscriptionDefinition, string Error);

    private record struct ApiEndpoints(string Log);
}