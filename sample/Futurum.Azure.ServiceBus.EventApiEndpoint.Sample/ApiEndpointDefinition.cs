using Futurum.ApiEndpoint;
using Futurum.Azure.ServiceBus.EventApiEndpoint.Metadata;
using Futurum.EventApiEndpoint;

namespace Futurum.Azure.ServiceBus.EventApiEndpoint.Sample;

public class ApiEndpointDefinition : IApiEndpointDefinition
{
    public void Configure(ApiEndpointDefinitionBuilder definitionBuilder)
    {
        definitionBuilder.Event()
                         .AzureServiceBus()
                         .Event<TestEventApiEndpoint.ApiEndpoint>(builder => builder.Queue("sample.servicebus"))
                         .EnvelopeEvent(builder => builder.FromQueue("sample.servicebus")
                                                          .Route<TestBatchRouteEventApiEndpoint.ApiEndpoint>("test-batch-route"));
    }
}