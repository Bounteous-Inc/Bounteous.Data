using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bounteous.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityProvider<TUserId>(this IServiceCollection services)
        where TUserId : struct
    {
        services.TryAddScoped<IIdentityProvider<TUserId>, IdentityProvider<TUserId>>();
        return services;
    }

    public static IServiceCollection AddSingletonIdentityProvider<TUserId>(this IServiceCollection services)
        where TUserId : struct
    {
        services.TryAddSingleton<IIdentityProvider<TUserId>, IdentityProvider<TUserId>>();
        return services;
    }
}
