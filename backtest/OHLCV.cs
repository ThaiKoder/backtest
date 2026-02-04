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
    public class OHLCV
    {
        [JsonPropertyName("hd")]
        public Header Hd { get; set; } = new Header();

        [JsonPropertyName("open")]
        [JsonConverter(typeof(ParseStringToDoubleConverter))]
        public double Open { get; set; }

        [JsonPropertyName("high")]
        [JsonConverter(typeof(ParseStringToDoubleConverter))]
        public double High { get; set; }

        [JsonPropertyName("low")]
        [JsonConverter(typeof(ParseStringToDoubleConverter))]
        public double Low { get; set; }

        [JsonPropertyName("close")]
        [JsonConverter(typeof(ParseStringToDoubleConverter))]
        public double Close { get; set; }

        [JsonPropertyName("volume")]
        [JsonConverter(typeof(ParseStringToDoubleConverter))]
        public double Volume { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;
    }

    public class Header
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
        // NOUVELLE méthode : normalisation d'une seule bougie
        public static OHLCV Normalize(OHLCV candle)
        {
            // 👉 mets ici ta logique de normalisation existante
            // ex: conversion prix, timestamps, etc.

            return candle;
        }

        // (optionnel) ancienne méthode si encore utilisée ailleurs
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
                            ohlcvs.Add(Normalize(ohlcv));
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




    internal class ParseStringToDoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    return value;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }

            throw new JsonException($"Impossible de convertir en double la valeur '{reader.GetString()}'");
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }

}