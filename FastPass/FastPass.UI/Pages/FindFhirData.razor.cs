using Hl7.Fhir.Model;

namespace FastPass.UI.Pages;

public partial class FindFhirData
{
    private Bundle FhirBundle { get; set; } = default!;

    private List<Patient?> Patients
    {
        get
        {
            return FhirBundle.Entry
                ?.Where(e => string.Equals(e?.Resource?.TypeName, "patient", StringComparison.OrdinalIgnoreCase) && e?.Resource is Patient)
                ?.Select(e => e.Resource as Patient)
                ?.Where(p => p != null)
                ?.ToList() ?? new List<Patient?>();
        }
    }

    private List<Observation?> Observations
    {
        get
        {
            return FhirBundle.Entry
                ?.Where(entryComponent => entryComponent.Resource.TypeName.ToLower() == "observation")
                ?.Select(entryComponent => entryComponent?.Resource as Observation)
                ?.Where(o => o != null)
                ?.ToList() ?? new List<Observation?>();
        }
    }
}
