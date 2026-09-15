using System.Text.Json;
using Deuna.Merchant.Sdk.Models.Enums;
using FluentAssertions;
using Xunit;

namespace Deuna.Merchant.Sdk.UnitTests;

public sealed class QrResponseFormatJsonConverterTests
{
    [Theory]
    [InlineData(QrResponseFormat.DeeplinkOnly, "\"0\"")]
    [InlineData(QrResponseFormat.QrOnly, "\"1\"")]
    [InlineData(QrResponseFormat.QrAndDeeplink, "\"2\"")]
    [InlineData(QrResponseFormat.NumericCode, "\"3\"")]
    [InlineData(QrResponseFormat.NumericCodeAndQr, "\"4\"")]
    [InlineData(QrResponseFormat.All, "\"5\"")]
    public void Serialize_WritesNumericString(QrResponseFormat format, string expectedJson)
    {
        var json = JsonSerializer.Serialize(format);
        json.Should().Be(expectedJson);
    }

    [Theory]
    [InlineData("\"0\"", QrResponseFormat.DeeplinkOnly)]
    [InlineData("\"1\"", QrResponseFormat.QrOnly)]
    [InlineData("\"2\"", QrResponseFormat.QrAndDeeplink)]
    [InlineData("\"3\"", QrResponseFormat.NumericCode)]
    [InlineData("\"4\"", QrResponseFormat.NumericCodeAndQr)]
    [InlineData("\"5\"", QrResponseFormat.All)]
    public void Deserialize_FromString_ReturnsExpectedEnum(string json, QrResponseFormat expectedFormat)
    {
        var format = JsonSerializer.Deserialize<QrResponseFormat>(json);
        format.Should().Be(expectedFormat);
    }

    [Theory]
    [InlineData("0", QrResponseFormat.DeeplinkOnly)]
    [InlineData("1", QrResponseFormat.QrOnly)]
    [InlineData("2", QrResponseFormat.QrAndDeeplink)]
    [InlineData("3", QrResponseFormat.NumericCode)]
    [InlineData("4", QrResponseFormat.NumericCodeAndQr)]
    [InlineData("5", QrResponseFormat.All)]
    public void Deserialize_FromInteger_ReturnsExpectedEnum(string json, QrResponseFormat expectedFormat)
    {
        var format = JsonSerializer.Deserialize<QrResponseFormat>(json);
        format.Should().Be(expectedFormat);
    }

    [Theory]
    [InlineData("\"99\"")]
    [InlineData("\"invalid\"")]
    [InlineData("99")]
    public void Deserialize_InvalidValue_ThrowsJsonException(string json)
    {
        var act = () => JsonSerializer.Deserialize<QrResponseFormat>(json);
        act.Should().Throw<JsonException>();
    }
}
