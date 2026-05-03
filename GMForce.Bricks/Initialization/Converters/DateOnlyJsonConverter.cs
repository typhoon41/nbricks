using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GMForce.Bricks.Initialization.Converters;

public sealed class DateOnlyJsonConverter : JsonConverterFactory
{
    private static readonly string[] SupportedFormats = ["d.M.yyyy.", "MM/dd/yyyy", "dd.MM.yyyy"];

    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(DateOnly) || typeToConvert == typeof(DateOnly?);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => typeToConvert == typeof(DateOnly?)
            ? new NullableConverter()
            : new NonNullableConverter();

    private sealed class NonNullableConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                throw new JsonException("Date value cannot be null.");
            }

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new JsonException("Date value is required.");
            }

            return DateOnly.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : throw new JsonException("Unsupported date format!");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(CultureInfo.CurrentCulture));
    }

    private sealed class NullableConverter : JsonConverter<DateOnly?>
    {
        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateOnly.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : throw new JsonException("Unsupported date format!");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Value.ToString(CultureInfo.CurrentCulture));
        }
    }
}
