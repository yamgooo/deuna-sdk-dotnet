using System.Text.Json;
using System.Text.Json.Serialization;
using Deuna.Merchant.Sdk.Models.Enums;

namespace Deuna.Merchant.Sdk.Serialization;

/// <summary>
/// Custom JSON converter that serializes <see cref="QrResponseFormat"/> as numeric strings
/// (<c>"0"</c>, <c>"1"</c>, <c>"2"</c>, <c>"3"</c>, <c>"4"</c>, <c>"5"</c>) as required by the DEUNA API,
/// and deserializes from either string or numeric JSON tokens for maximum compatibility.
/// </summary>
public sealed class QrResponseFormatJsonConverter : JsonConverter<QrResponseFormat>
{
    /// <inheritdoc/>
    public override QrResponseFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringVal = reader.GetString();
            if (int.TryParse(stringVal, out var intVal) && Enum.IsDefined(typeof(QrResponseFormat), intVal))
            {
                return (QrResponseFormat)intVal;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            var intVal = reader.GetInt32();
            if (Enum.IsDefined(typeof(QrResponseFormat), intVal))
            {
                return (QrResponseFormat)intVal;
            }
        }

        throw new JsonException(
            $"Unable to parse value '{reader.GetString()}' as {nameof(QrResponseFormat)}. " +
            "Expected numeric string or integer representing a valid format (0-5).");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, QrResponseFormat value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(((int)value).ToString());
    }
}
