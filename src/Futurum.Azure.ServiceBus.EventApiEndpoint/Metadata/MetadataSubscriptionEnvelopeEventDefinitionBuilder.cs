using Futurum.ApiEndpoint.DebugLogger;
using Futurum.EventApiEndpoint.Metadata;

namespace Futurum.Azure.ServiceBus.EventApiEndpoint.Metadata;

public class MetadataSubscriptionEnvelopeEventDefinitionBuilder
{
    private string? _fromQueue;
    private readonly List<MetadataSubscriptionEnvelopeEventRouteDefinitionBuilder> _routes = new();

    public MetadataSubscriptionEnvelopeEventRouteDefinitionBuilder FromQueue(string fromQueue)
    {
        _fromQueue = fromQueue;

        var envelopeEventRouteDefinitionBuilder = new MetadataSubscriptionEnvelopeEventRouteDefinitionBuilder(fromQueue);

        _routes.Add(envelopeEventRouteDefinitionBuilder);

        return envelopeEventRouteDefinitionBuilder;
    }

    public IEnumerable<MetadataSubscriptionEnvelopeEventDefinition> Build() =>
        _routes.SelectMany(x => x.Build());

    public ApiEndpointDebugNode Debug() =>
        new()
        {
            Name = $"{_fromQueue} (ENVELOPE)",
            Children = _routes.SelectMany(x => x.Debug()).ToList()
        };
}