namespace FastPass.Api.TextAnalyticsModels;

public class TextAnalyticsRequest
{
    public TextAnalyticsRequest(string text, string kind = "Healthcare", string fhirVersion = "4.0.1", string language = "en", string documentId = "")
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
        AnalysisInput = new TextAnalyticsAnalysisInput
        {
            Documents = new List<TextAnalyticsDocument>()
            {
                new TextAnalyticsDocument
                {
                    Id = documentId,
                    Language = language,
                    Text = text
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
