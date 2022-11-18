using FastPass.API.Services;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace FastPass.API
{
    public class FhirServiceProxyFunction
    {
        private static JsonSerializerSettings _jsonsettings;
        private readonly IFirelyService _fhirService;
        private readonly ILogger _log;

        public FhirServiceProxyFunction(
            IOptions<ConfigurationModel> config, 
            ILoggerFactory loggerFactory,
            JsonSerializerSettings jsonSettings, 
            IFirelyService fhirService)
        {
            _jsonsettings = jsonSettings;
            _fhirService = fhirService;
            _log = loggerFactory.CreateLogger<FhirServiceProxyFunction>();
        }

        [Function("AddPatient")]
        public async Task<HttpResponseData> AddPatient([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            try
            {
                var bodyString = await req.GetBodyString();
                var parser = new FhirJsonParser();
                var patient = parser.Parse<Patient>(bodyString);

                _log.Log(LogLevel.Trace, "Calling FhirService::AddPatient");
                var returnedPatient = await _fhirService.CreatePatientAsync(patient);

                _log.Log(LogLevel.Trace, $"FhirService::AddPatient succeeded. {returnedPatient.Id} created.");

                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteAsJsonAsync(returnedPatient);
                return resp;
            }
            catch (Exception ex)
            {
                var msg = $"FhirService::AddPatient failed. Detail: {ex}";
                _log.Log(LogLevel.Error, msg);

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

                _log.Log(LogLevel.Trace, $"Calling FhirService::UpdatePatient for id {patient.Id}");
                var returnedPatient = await _fhirService.UpdatePatientAsync(patient.Id, patient);

                _log.Log(LogLevel.Trace, $"FhirService::UpdatePatient succeeded. {patient.Id} updated.");

                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteAsJsonAsync(returnedPatient);
                return resp;
            }
            catch (Exception ex)
            {
                var msg = $"FhirService::AddPatient failed. Detail: {ex}";
                _log.Log(LogLevel.Error, msg);

                var err = req.CreateResponse(HttpStatusCode.BadRequest);
                await err.WriteStringAsync(msg);
                return err;
            }
        }
    }
}
