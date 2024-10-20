using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nefarious.Common.Options;

namespace Nefarious.Core.Extensions;

public static class DiscordServiceCollectionExtensions
{
    private static IServiceCollection AddDiscordCoreServices(this IServiceCollection services) =>
        services
            .AddSingleton<InteractionService>(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandService>(sp => {
                var options = sp.GetRequiredService<IOptions<DiscordOptions>>().Value;
                return new CommandService(new CommandServiceConfig
                {
                    LogLevel = options.LogLevel,
                    DefaultRunMode = options.CommandService.DefaultRunMode
                });
            });

    public static IServiceCollection AddDiscordWebsocketClient(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddSingleton<DiscordSocketClient>(sp => {
                var options = sp.GetRequiredService<IOptions<DiscordOptions>>().Value;
                return new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = options.GatewayIntents,
                    DefaultRetryMode = options.DefaultRetryMode,
                    AlwaysDownloadUsers = options.AlwaysDownloadUsers,
                    MessageCacheSize = options.MessageCacheSize,
                    LogLevel = options.LogLevel,
                    LogGatewayIntentWarnings = options.LogGatewayIntentWarnings
                });
            })
            .AddDiscordCoreServices();
}