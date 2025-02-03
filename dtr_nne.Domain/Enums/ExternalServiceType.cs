using System.Text.Json.Serialization;

namespace dtr_nne.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<ExternalServiceType>))]
public enum ExternalServiceType
{
    Llm = 0,
    Translator = 1,
    Scraper = 2
}