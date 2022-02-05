using System.Reflection;

using Futurum.EventApiEndpoint;
using Futurum.Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Futurum.Azure.ServiceBus.EventApiEndpoint;

public class AzureServiceBusEventApiEndpointModule : IModule
{
    private readonly EventApiEndpointConfiguration _configuration;
    private readonly AzureServiceBusConnectionConfiguration _connectionConfiguration;
    private readonly Assembly[] _assemblies;

    public AzureServiceBusEventApiEndpointModule(EventApiEndpointConfiguration configuration,
                                                 AzureServiceBusConnectionConfiguration connectionConfiguration,
                                                 params Assembly[] assemblies)
    {
        _configuration = configuration;
        _connectionConfiguration = connectionConfiguration;
        _assemblies = assemblies;
    }

    public AzureServiceBusEventApiEndpointModule(AzureServiceBusConnectionConfiguration connectionConfiguration,
                                                 params Assembly[] assemblies)
        : this(EventApiEndpointConfiguration.Default, connectionConfiguration, assemblies)
    {
    }

    public void Load(IServiceCollection services)
    {
        services.RegisterModule(new EventApiEndpointModule(_configuration, _assemblies));

        services.AddSingleton(_connectionConfiguration);

        services.AddSingleton<IEventApiEndpointLogger, EventApiEndpointLogger>();
        services.AddSingleton<Futurum.EventApiEndpoint.IEventApiEndpointLogger, EventApiEndpointLogger>();
        services.AddSingleton<ApiEndpoint.IApiEndpointLogger, EventApiEndpointLogger>();

        services.AddSingleton<IAzureServiceBusEventApiEndpointWorkerService, AzureServiceBusEventApiEndpointWorkerService>();
    }
}