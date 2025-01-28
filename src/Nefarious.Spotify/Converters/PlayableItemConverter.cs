using SpotifyAPI.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nefarious.Spotify.Converters;

public class SystemPlayableItemConverter : JsonConverter<IPlayableItem>
{
    public override IPlayableItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        if (!doc.RootElement.TryGetProperty("type", out var typeElement))
            throw new JsonException("Unable to determine the type of IPlayableItem.");

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        // Extremely niche case where I only need string or a number
        var type = typeElement.ValueKind switch
        {
            JsonValueKind.String => typeElement.GetString(),
            JsonValueKind.Number => Enum.GetName(typeof(ItemType), typeElement.GetInt32()),
            _ => throw new JsonException("Unable to determine the type of IPlayableItem")
        };

        return type switch
        {
            nameof(ItemType.Track) => doc.RootElement.Deserialize<FullTrack>(options),
            nameof(ItemType.Episode) => doc.RootElement.Deserialize<FullEpisode>(options),
            _ => throw new NotSupportedException($"Unsupported item type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, IPlayableItem value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}