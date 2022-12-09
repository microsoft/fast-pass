using Hl7.Fhir.Model;

namespace FastPass.Models;

public class TextAnalyticsResult
{
    public List<Entity> Entities { get; set; } = default!;
    // we'll have to use fhir parser on it in the front end anyway but from an api perspective it would be nice to return something that would be useful without a front end
    public string FhirBundle { get; set; } = default!;
}
