using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nefarious.Common.Options;
using Nefarious.Common.Services.Discord;

namespace Nefarious.Core.Services;

public class NefariousBotService : DiscordWebsocketService<NefariousBotService>
{
    public NefariousBotService(DiscordSocketClient client, ILogger<NefariousBotService> logger, IOptions<DiscordOptions> options)
        : base(client, logger, options)
    {

    }
}