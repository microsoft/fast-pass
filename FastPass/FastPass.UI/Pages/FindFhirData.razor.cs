using Hl7.Fhir.Model;

namespace FastPass.UI.Pages;

public partial class FindFhirData
{

    private Bundle FhirBundle { get; set; } = default!;

    private List<Patient> Patients
    {
        get
        {
            return FhirBundle.Entry
        ?.Where(e => string.Equals(e?.Resource?.TypeName, "patient", StringComparison.OrdinalIgnoreCase) && e.Resource is Patient)
        ?.Select(e => e.Resource as Patient)
        ?.Where(p => p != null)
        ?.ToList() ?? new List<Patient>();
        }
    }

    //protected override async Task OnInitializedAsync()
    //{
    //}


    /*
            var observations = bundle.Entry.Where(x => x.Resource.TypeName.ToLower() == "observation").Select(x => (Observation)x.Resource).ToList();
     */
}
