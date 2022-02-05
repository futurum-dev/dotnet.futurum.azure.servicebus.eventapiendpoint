using Futurum.ApiEndpoint;
using Futurum.ApiEndpoint.DebugLogger;
using Futurum.EventApiEndpoint.Metadata;

namespace Futurum.Azure.ServiceBus.EventApiEndpoint.Metadata;

public class MetadataSubscriptionDefinitionBuilder
{
    private readonly Type _apiEndpointType;
    private string _queue;

    public MetadataSubscriptionDefinitionBuilder(Type apiEndpointType)
    {
        _apiEndpointType = apiEndpointType;
    }

    public MetadataSubscriptionDefinitionBuilder Queue(string queue)
    {
        _queue = queue;

        return this;
    }

    public IEnumerable<IMetadataDefinition> Build()
    {
        yield return new MetadataSubscriptionEventDefinition(new MetadataTopic(_queue));
    }

    public ApiEndpointDebugNode Debug() =>
        new()
        {
            Name = $"{_queue} ({_apiEndpointType.FullName})"
        };
}