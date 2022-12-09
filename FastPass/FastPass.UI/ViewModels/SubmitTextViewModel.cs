using System;
using FastPass.Models;
using Hl7.Fhir.Model;

namespace FastPass.UI.ViewModels
{
	public class SubmitTextViewModel
	{
        internal TextAnalyticsProxyRequest textAnalyticsProxyRequest = new TextAnalyticsProxyRequest { };
        internal Patient? patient { get; set; }

        internal string PatientFirstName
        {
            get
            {
                return patient?.Name[0]?.Given?.FirstOrDefault() ?? string.Empty;
            }
        }
    }
}

