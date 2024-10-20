namespace Nefarious.Common.Options;

public class SpotifyOptions : BaseOptions<SpotifyOptions>
{
    public required string ClientSecret { get; init; }
    public required string ClientId { get; init; }
}