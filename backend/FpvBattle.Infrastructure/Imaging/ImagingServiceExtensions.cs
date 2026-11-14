using FpvBattle.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Infrastructure.Imaging;

public static class ImagingServiceExtensions
{
    public static IServiceCollection AddImaging(this IServiceCollection services)
    {
        services.AddScoped<IWinnerImageRenderer, WinnerImageRenderer>();

        return services;
    }
}
