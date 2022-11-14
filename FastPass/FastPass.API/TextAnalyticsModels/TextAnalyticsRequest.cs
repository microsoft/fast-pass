using System;
using System.Collections.Generic;

namespace FastPass.API.TextAnalyticsModels
{
    public class TextAnalyticsRequest
    {
        public TextAnalyticsRequest(string kind = "Heathcare", string fhirVersion = "4.0.1", string documentId = "")
        {
            Tasks = new List<TextAnalyticsTask>()
            {
                new TextAnalyticsTask
                {
                    Kind = kind,
                    Parameters = new TextAnalyticsParameters
                    {
                        FhirVersion = fhirVersion
                    }
                }
            };
        }

        public List<TextAnalyticsTask> Tasks { get; set; }
        public TextAnalyticsAnalysisInput AnalysisInput { get; set; }

    }

    public class TextAnalyticsParameters
    {
        public string FhirVersion { get; set; }
    }

    public class TextAnalyticsAnalysisInput
    {
        public List<TextAnalyticsDocument> Documents { get; set; }
    }

    public class TextAnalyticsDocument
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Language { get; set; }
    }

    public class TextAnalyticsTask
    {
        public string Kind { get; set; }
        public TextAnalyticsParameters Parameters { get; set; }
    }
}
