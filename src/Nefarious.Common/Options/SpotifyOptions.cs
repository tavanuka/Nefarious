namespace Nefarious.Common.Options;

public class SpotifyOptions : BaseOptions<SpotifyOptions>
{
    public required string ApiKey { get; init; }
}