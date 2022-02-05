namespace Futurum.Azure.ServiceBus.EventApiEndpoint.Metadata;

public static class EventApiEndpointDefinitionExtensions
{
    public static EventApiEndpointDefinition AzureServiceBus(this Futurum.EventApiEndpoint.EventApiEndpointDefinition eventApiEndpointDefinition)
    {
        var azureServiceBusEventApiEndpointDefinition = new EventApiEndpointDefinition();
        
        eventApiEndpointDefinition.Add(azureServiceBusEventApiEndpointDefinition);

        return azureServiceBusEventApiEndpointDefinition;
    }
}