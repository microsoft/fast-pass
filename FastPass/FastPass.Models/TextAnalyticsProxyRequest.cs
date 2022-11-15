using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ResourceType = Hl7.Fhir.Model.ResourceType;

namespace FastPass.Models
{
    public class TextAnalyticsProxyRequest
    {
        public string? TextToAnalize { get; set; }

//        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<ResourceType>? FhirResourceTypesToReturn { get; set; }
        public string? Id { get; set; }
        public string? Language { get; set; }
    }
}