using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace backtest
{
    //exemple: {"hd":{"ts_event":"2022-06-10T12:30:00.000000000Z","rtype":33,"publisher_id":2,"instrument_id":7370},"open":"265.760000000","high":"265.760000000","low":"261.010000000","close":"261.060000000","volume":"3006","symbol":"MSFT"}
    internal class OHLCV
    {
        [JsonPropertyName("hd")]
        public Header Hd { get; set; } = new Header();

        [JsonPropertyName("open")]
        [JsonConverter(typeof(ParseStringToDecimalConverter))]
        public decimal Open { get; set; }

        [JsonPropertyName("high")]
        [JsonConverter(typeof(ParseStringToDecimalConverter))]
        public decimal High { get; set; }

        [JsonPropertyName("low")]
        [JsonConverter(typeof(ParseStringToDecimalConverter))]
        public decimal Low { get; set; }

        [JsonPropertyName("close")]
        [JsonConverter(typeof(ParseStringToDecimalConverter))]
        public decimal Close { get; set; }

        [JsonPropertyName("volume")]
        [JsonConverter(typeof(ParseStringToLongConverter))]
        public long Volume { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;
    }

    internal class Header
    {
        [JsonPropertyName("ts_event")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("rtype")]
        public int RType { get; set; }

        [JsonPropertyName("publisher_id")]
        public int PublisherId { get; set; }

        [JsonPropertyName("instrument_id")]
        public int InstrumentId { get; set; }
    }

    internal class OHLCVNormalizer
    {
        // Méthode pour lire un fichier et retourner tous les OHLCV normalisés
        public static List<OHLCV> Normalize(string filePath)
        {
            var ohlcvs = new List<OHLCV>();

            foreach (var line in File.ReadLines(filePath))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    try
                    {
                        OHLCV? ohlcv = JsonSerializer.Deserialize<OHLCV>(line);
                        if (ohlcv != null)
                            ohlcvs.Add(ohlcv);
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine($"Erreur de désérialisation : {ex.Message}");
                    }
                }
            }

            return ohlcvs;
        }
    }




    internal class ParseStringToDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    return value;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }

            throw new JsonException($"Impossible de convertir en decimal la valeur '{reader.GetString()}'");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }

    internal class ParseStringToLongConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    return value;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }

            throw new JsonException($"Impossible de convertir en long la valeur '{reader.GetString()}'");
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}