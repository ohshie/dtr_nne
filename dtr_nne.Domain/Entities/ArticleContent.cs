using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dtr_nne.Domain.Entities;

public class ArticleContent
{
    public int Id { get; set; }

    [DefaultValue("")]
    [MaxLength(30000)]
    [JsonPropertyName("body")]
    [JsonConverter(typeof(JsonStringOrArrayConverter<string>))]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    [JsonConverter(typeof(JsonUriListConverter))]
    public List<Uri> Images { get; set; } = [];

    [JsonPropertyName("copyright")]
    [JsonConverter(typeof(JsonStringOrArrayConverter<List<string>>))]
    public List<string> Copyright { get; set; } = [];
    
    [MaxLength(200)]
    [JsonPropertyName("source")]
    [JsonConverter(typeof(JsonStringOrArrayConverter<string>))]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("header")]
    [JsonConverter(typeof(HeadlineConverter))]
    public Headline Headline { get; set; } = new();
    
    public EditedArticle? EditedArticle { get; set; }
}

internal class JsonStringOrArrayConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var str = reader.GetString() ?? string.Empty;
                return typeToConvert == typeof(List<string>) 
                    ? (T)(object)new List<string> { str }
                    : (T)(object)str;
            case JsonTokenType.StartArray:
            {
                var list = new List<string>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        list.Add(reader.GetString() ?? string.Empty);
                    }
                }
                return typeToConvert == typeof(List<string>) 
                    ? (T)(object)list 
                    : (T)(object)string.Join(" ", list);
            }
            default:
                return typeToConvert == typeof(List<string>) 
                    ? (T)(object)new List<string>() 
                    : (T)(object)string.Empty;
        }
    }
    
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is List<string> list)
        {
            writer.WriteStartArray();
            foreach (var item in list)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
        }
        else
        {
            writer.WriteStringValue(value?.ToString() ?? string.Empty);
        }
    }
}

internal class JsonUriListConverter : JsonConverter<List<Uri>>
{
    public override List<Uri> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var results = new List<Uri>();
        
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var uriString = reader.GetString();
                if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
                {
                    results.Add(uri);
                }
                break;
                
            case JsonTokenType.StartArray:
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        var arrayUriString = reader.GetString();
                        if (Uri.TryCreate(arrayUriString, UriKind.Absolute, out var arrayUri))
                        {
                            results.Add(arrayUri);
                        }
                    }
                }
                break;
        }
        
        return results;
    }

    public override void Write(Utf8JsonWriter writer, List<Uri> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var uri in value)
        {
            writer.WriteStringValue(uri.ToString());
        }
        writer.WriteEndArray();
    }
}

internal class HeadlineConverter : JsonConverter<Headline>
{
    public override Headline? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new Headline
            {
                OriginalHeadline = reader.GetString() ?? string.Empty,
            };
        }
        
        return null;
    }

    public override void Write(Utf8JsonWriter writer, Headline value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.OriginalHeadline);
    }
}
