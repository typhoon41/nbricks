using System.Text.Json;
using GMForce.Bricks.Initialization.Converters;
using GMForce.Bricks.Tests.Arrangement.Codebooks;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Initialization.Converters;

internal sealed class DateOnlyJsonConverterFixture
{
    private static JsonSerializerOptions WithConverter() => new() { Converters = { new DateOnlyJsonConverter() } };

    [TestCase(typeof(DateOnly), true)]
    [TestCase(typeof(DateOnly?), true)]
    [TestCase(typeof(DateTime), false)]
    public void CanConvert(Type type, bool expected)
    {
        var converter = new DateOnlyJsonConverter();

        var result = converter.CanConvert(type);

        result.ShouldBe(expected);
    }

    [TestCase(Dates.SlashFormat)]
    [TestCase(Dates.DotFormat)]
    [TestCase(Dates.ShortDotFormat)]
    public void ReadNonNullableSupportsAllFormats(string format)
    {
        var result = JsonSerializer.Deserialize<DateOnly>($"\"{format}\"", WithConverter());

        result.ShouldBe(Dates.ExpectedDate);
    }

    [TestCase("null")]
    [TestCase("\"\"")]
    [TestCase("\"2024-01-15\"")]
    public void ReadNonNullableInvalidInputThrows(string json)
    {
        void act()
        {
            JsonSerializer.Deserialize<DateOnly>(json, WithConverter());
        }

        Should.Throw<JsonException>(act);
    }

    [Test]
    public void WriteNonNullableWritesString()
    {
        var json = JsonSerializer.Serialize(Dates.ExpectedDate, WithConverter());

        json.ShouldNotBeNullOrEmpty();
    }

    [TestCase("null")]
    [TestCase("\"\"")]
    public void ReadNullableReturnsNullForEmptyOrNullToken(string json)
    {
        var result = JsonSerializer.Deserialize<DateOnly?>(json, WithConverter());

        result.ShouldBeNull();
    }

    [Test]
    public void ReadNullableValidFormatParses()
    {
        var result = JsonSerializer.Deserialize<DateOnly?>($"\"{Dates.SlashFormat}\"", WithConverter());

        result.ShouldBe(Dates.ExpectedDate);
    }

    [Test]
    public void WriteNullableNullWritesNullValue()
    {
        var json = JsonSerializer.Serialize<DateOnly?>(null, WithConverter());

        json.ShouldBe("null");
    }

    [Test]
    public void WriteNullableNonNullWritesString()
    {
        var json = JsonSerializer.Serialize<DateOnly?>(Dates.ExpectedDate, WithConverter());

        json.ShouldNotBeNullOrEmpty();
    }
}
