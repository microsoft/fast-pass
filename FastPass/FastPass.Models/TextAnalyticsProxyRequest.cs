using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using ResourceType = Hl7.Fhir.Model.ResourceType;

namespace FastPass.Models
{
    public class TextAnalyticsProxyRequest
    {
        public string? TextToAnalyze { get; set; }
        public string? Id { get; set; }
        public string? Language { get; set; }
    }
}