using FastPass.API.Services;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastPass.API
{
    public class FhirServiceProxyFunction
    {
        private static JsonSerializerSettings _jsonsettings;
        private readonly IFirelyService _fhirService;

        public FhirServiceProxyFunction(IOptions<ConfigurationModel> config, JsonSerializerSettings jsonSettings, IFirelyService fhirService)
        {
            _jsonsettings = jsonSettings;
            _fhirService = fhirService;
        }

        [FunctionName("AddPatient")]
        public async Task<IActionResult> AddPatient([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                var bodyString = await req.GetBodyString();
                var parser = new FhirJsonParser();
                var patient = parser.Parse<Patient>(bodyString);

                log.Log(LogLevel.Trace, "Calling FhirService::AddPatient");
                var returnedPatient = await _fhirService.CreatePatientAsync(patient);

                log.Log(LogLevel.Trace, $"FhirService::AddPatient succeeded. {returnedPatient.Id} created.");
                return new OkObjectResult(returnedPatient);
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, $"FhirService::AddPatient failed. Detail: {ex}");
                return new StatusCodeResult(500);
            }
        }

        [FunctionName("UpdatePatient")]
        public async Task<IActionResult> UpdatePatient([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = null)] HttpRequest req, ILogger log)
        {
            try
            {
                var bodyString = await req.GetBodyString();
                var id = req.Path.Value?.Split('/')?.Last();
                var parser = new FhirJsonParser();
                var patient = parser.Parse<Patient>(bodyString);

                log.Log(LogLevel.Trace, $"Calling FhirService::UpdatePatient for id {patient.Id}");
                var returnedPatient = await _fhirService.UpdatePatientAsync(patient.Id, patient);

                log.Log(LogLevel.Trace, $"FhirService::UpdatePatient succeeded. {patient.Id} updated.");
                return new OkObjectResult(returnedPatient);
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, $"FhirService::AddPatient failed. Detail: {ex}");
                return new StatusCodeResult(500);
            }
        }
    }
}
