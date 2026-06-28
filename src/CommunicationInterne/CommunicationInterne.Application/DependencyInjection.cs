using FluentValidation;
using MDiator;
using Microsoft.Extensions.DependencyInjection;

namespace Cnss.Metier.CommunicationInterne.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCommunicationInterneApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddMDiator(assembly);
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
}
