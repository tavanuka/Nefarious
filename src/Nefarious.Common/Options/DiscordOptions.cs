using Discord;

namespace Nefarious.Common.Options;

public class DiscordOptions : BaseOptions<DiscordOptions>
{

    public required Dictionary<string, string> AccessTokens { get; init; }
    public required GatewayIntents GatewayIntents { get; init; }
    public required RetryMode DefaultRetryMode { get; init; }
    public required bool AlwaysDownloadUsers { get; init; }
    public required LogSeverity LogLevel { get; init; }
    public required bool LogGatewayIntentWarnings { get; init; }
    public required int MessageCacheSize { get; init; }
    public required DiscordCommandOptions CommandService { get; init; }
    public required string ClientId { get; init; }
}