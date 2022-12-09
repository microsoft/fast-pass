using Azure.Core;
using FastPass.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Task = System.Threading.Tasks.Task;

namespace FastPass.UI.Services;

public class ApiService
{
    private const string TEXT_ANALYTICS_ENDPOINT = "api/TextAnalyticsServiceProxy";
    private const string FHIR_PATIENT_SUBMISSION_ENDPOINT = "api/AddPatient";
    private const string FHIR_OBSERVATION_SUBMISSION_ENDPOINT = "api/AddObservation";
    private readonly HttpClient _client;
    private FhirJsonParser _parser = new();

    public ApiService(HttpClient client)
    {
        _client = client;
    }

    private async Task<List<TextAnalyticsResult>> AnalyzeTextAsync(string text)
    {
        var result = await _client.PostAsync(TEXT_ANALYTICS_ENDPOINT, new StringContent(text));

        return await result!.Content!.ReadFromJsonAsync<List<TextAnalyticsResult>>() ?? new List<TextAnalyticsResult>();
    }

    public async Task<Bundle> FindFhirDataAsync(TextAnalyticsProxyRequest request)
    {
        var text = string.IsNullOrEmpty(request.TextToAnalyze) ? "" : request.TextToAnalyze;
        var result = await _client.PostAsync(TEXT_ANALYTICS_ENDPOINT, new StringContent(text));

        var textAnalyticsResults = await result.Content.ReadFromJsonAsync<List<TextAnalyticsResult>>();

        // 1) we haven't currently explored the ramifications of combining fhir bundles so for simplicity in our demo we are just using the first one
        // 2) Firely's implementation of Bundles/other fhir resources need to be serialized/deserialized with special serializers and parsers
        // so we're converting it back to raw text to give to their special parser

        if (string.IsNullOrWhiteSpace(textAnalyticsResults?.FirstOrDefault()?.FhirBundle))
        {
            Console.WriteLine("wut");
        }

        return await _parser.ParseAsync<Bundle>(textAnalyticsResults?.FirstOrDefault()?.FhirBundle);
    }

    public async Task<List<Entity>> FindEntitiesAsync(TextAnalyticsProxyRequest request)
    {
        var text = string.IsNullOrEmpty(request.TextToAnalyze) ? "" : request.TextToAnalyze;

        var textAnalyticsResults = await AnalyzeTextAsync(text);

        return textAnalyticsResults.SelectMany(r => r.Entities).ToList();
    }

    public async Task<Patient> SubmitPatientAsync(Patient patient)
    {
        var json = await patient.ToJsonAsync();

        var result = await _client.PostAsync(FHIR_PATIENT_SUBMISSION_ENDPOINT, new StringContent(json));

        var returnedJson = await result.Content.ReadAsStringAsync();

        return await _parser.ParseAsync<Patient>(returnedJson);
    }

    public async Task<Observation> SubmitObservationAsync(Observation observation)
    {
        var json = await observation.ToJsonAsync();

        var result = await _client.PostAsync(FHIR_OBSERVATION_SUBMISSION_ENDPOINT, new StringContent(json));

        var returnedJson = await result.Content.ReadAsStringAsync();

        return await _parser.ParseAsync<Observation>(returnedJson);
    }
}
