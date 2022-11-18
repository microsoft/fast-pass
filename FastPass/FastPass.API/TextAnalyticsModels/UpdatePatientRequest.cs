using Hl7.Fhir.Model;

namespace FastPass.API.TextAnalyticsModels
{
    public class UpdatePatientRequest
    {
        public Patient Patient { get; set; }
        public string Id { get; set;}
    }
}
