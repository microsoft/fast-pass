using FastPass.API.Services;
using FastPass.API.TextAnalyticsModels;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace FastPass.API.Functions
{
    public class FhirServiceProxyFunction
    {
        private static JsonSerializerSettings _jsonsettings;
        private readonly IFirelyService _fhirService;
        private readonly ILogger _logger;

        public FhirServiceProxyFunction(
            IOptions<ConfigurationModel> config,
            ILoggerFactory loggerFactory,
            JsonSerializerSettings jsonSettings,
            IFirelyService fhirService)
        {
            _jsonsettings = jsonSettings;
            _fhirService = fhirService;
            _logger = loggerFactory.CreateLogger<FhirServiceProxyFunction>();
        }

        [Function("Patient")]
        public async Task<HttpResponseData> GetPatient([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Patient/{id?}")] HttpRequestData req)
        {
            try
            {
                var idFromUri = req.Url.LocalPath.Split('/').Last().Replace("Patient", "");
                IList<Patient> returnedPatients = null;
                if (!string.IsNullOrWhiteSpace(idFromUri))
                {
                    var filter = new PatientFilters
                    {
                        FhirId = idFromUri
                    };
                    _logger.LogInformation("FhirService::GetPatient called for {patientId}.", idFromUri);

                    returnedPatients = await _fhirService.GetPatientsAsync(filter);
                }
                else
                {
                    _logger.LogInformation("FhirService::GetPatient called for all patients.");
                    returnedPatients = await _fhirService.GetPatientsAsync();
                }

                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteAsJsonAsync(returnedPatients);
                return resp;
            }
            catch (Exception ex)
            {
                var msg = $"FhirService::GetPatient failed. Detail: {ex}";
                _logger.LogError("FhirService::GetPatient failed. Detail: {MessageException}", msg);

                var err = req.CreateResponse(HttpStatusCode.BadRequest);
                await err.WriteStringAsync(msg);
                return err;
            }


        }

        [Function("AddPatient")]
        public async Task<HttpResponseData> AddPatient([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            try
            {
                var bodyString = await req.GetBodyString();
                var parser = new FhirJsonParser();
                var patient = parser.Parse<Patient>(bodyString);

                _logger.LogInformation("Calling FhirService::AddPatient");
                var returnedPatient = await _fhirService.CreatePatientAsync(patient);
                _logger.LogInformation("FhirService::AddPatient succeeded. {patientId} created.", returnedPatient.Id);

                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteAsJsonAsync(returnedPatient);
                return resp;
            }
            catch (Exception ex)
            {
                var msg = $"FhirService::AddPatient failed. Detail: {ex}";
                _logger.LogError(msg);

                var err = req.CreateResponse(HttpStatusCode.BadRequest);
                await err.WriteStringAsync(msg);
                return err;
            }
        }

        [Function("UpdatePatient")]
        public async Task<HttpResponseData> UpdatePatient([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = null)] HttpRequestData req)
        {
            try
            {
                var bodyString = await req.GetBodyString();
                var parser = new FhirJsonParser();
                var patient = parser.Parse<Patient>(bodyString);

                _logger.LogInformation("Calling FhirService::UpdatePatient for id {patientId}", patient.Id);
                var returnedPatient = await _fhirService.UpdatePatientAsync(patient.Id, patient);

                _logger.LogInformation("FhirService::UpdatePatient succeeded. {patientId} updated.", patient.Id);

                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteAsJsonAsync(returnedPatient);
                return resp;
            }
            catch (Exception ex)
            {
                var msg = $"FhirService::UpdatePatient failed. Detail: {ex}";
                _logger.LogError(msg);

                var err = req.CreateResponse(HttpStatusCode.BadRequest);
                await err.WriteStringAsync(msg);
                return err;
            }
        }
    }
}
