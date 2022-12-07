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

                    _logger.LogInformation($"FhirService::GetPatient called for {idFromUri}.");

                    returnedPatients = await _fhirService.GetPatientsAsync(filter);
                }
                else
                {
                    _logger.LogInformation("FhirService::GetPatient called for all patients.");

                    returnedPatients = await _fhirService.GetPatientsAsync();
                }

                var json = JsonConvert.SerializeObject(returnedPatients);

                return await req.CreateResponseAsync(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                var msg = $"FhirService::GetPatient failed. Detail: {ex}";

                _logger.LogError("FhirService::GetPatient failed. Detail: {MessageException}", msg);

                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, msg);
            }
        }

        [Function("AddPatient")]
        public async Task<HttpResponseData> AddPatient([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var fhirFx = $"{nameof(_fhirService)}::{nameof(_fhirService.CreatePatientAsync)}";

            try
            {
                var bodyString = await req.GetBodyString();

                var parser = new FhirJsonParser();

                var patient = parser.Parse<Patient>(bodyString);

                _logger.LogInformation($"Calling {fhirFx}");

                var returnedPatient = await _fhirService.CreatePatientAsync(patient);

                _logger.LogInformation($"{fhirFx} succeeded. {returnedPatient.Id} created.");

                var json = JsonConvert.SerializeObject(returnedPatient);

                return await req.CreateResponseAsync(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                var msg = $"{fhirFx} failed. Detail: {ex}";

                _logger.LogError(msg);

                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, msg);
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

        [Function("AddObservation")]
        public async Task<HttpResponseData> AddObservation([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            var fhirFx = $"{nameof(_fhirService)}::{nameof(_fhirService.CreateObservationAsync)}";

            try
            {
                var bodyString = await req.GetBodyString();

                var parser = new FhirJsonParser();

                var observation = parser.Parse<Observation>(bodyString);

                _logger.LogInformation($"Calling {fhirFx}");

                var returnedObservation = await _fhirService.CreateObservationAsync(observation);

                _logger.LogInformation($"{fhirFx} succeeded. {returnedObservation.Id} created.");

                var json = JsonConvert.SerializeObject(returnedObservation);
                
                return await req.CreateResponseAsync(HttpStatusCode.OK, json);
            }
            catch (Exception ex)
            {
                var msg = $"{fhirFx} failed. Detail: {ex}";

                _logger.LogError(msg);

                return await req.CreateResponseAsync(HttpStatusCode.BadRequest, msg);
            }
        }
    }
}
