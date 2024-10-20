using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nefarious.Common.Options;
using SpotifyAPI.Web;

namespace Nefarious.Spotify.Extensions;

public static class SpotifyServiceCollectionExtensions
{
    public static IServiceCollection AddSpotifyClient(this IServiceCollection services)
    {
        services.AddSingleton(sp => {
            var config = sp.GetRequiredService<IOptions<SpotifyOptions>>().Value;
            return SpotifyClientConfig.CreateDefault(config.ApiKey);
        });
 
        services.AddSingleton<ISpotifyClient>(sp => new SpotifyClient(sp.GetRequiredService<SpotifyClientConfig>()));
        return services;
    }
}