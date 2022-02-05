using Futurum.ApiEndpoint;
using Futurum.EventApiEndpoint.Metadata;

namespace Futurum.Azure.ServiceBus.EventApiEndpoint.Metadata;

public record MetadataSubscriptionEventDefinition(MetadataTopic Queue) : IMetadataDefinition;