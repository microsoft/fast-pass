using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace FastPass.API.TextAnalyticsModels
{
    public class TextAnalyticsResponse
    {
        public string JobId { get; set; }
        public DateTime? LastUpdatedDateTime { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? ExpirationDateTime { get; set; }
        public string Status { get; set; }
        public TextAnalyticsTasks Tasks { get; set; }

        public class Document
        {
            // Keep the bundle as a generic JObject to reduce possible serialization errors
            public JObject FhirBundle { get; set; }
        }

        public class Item
        {
            public Results Results { get; set; }
        }

        public class Results
        {
            public List<Document> Documents { get; set; }
        }

        public class TextAnalyticsTasks
        {
            public int? Completed { get; set; }
            public int? Failed { get; set; }
            public int? InProgress { get; set; }
            public int? Total { get; set; }
            public List<Item> Items { get; set; }
        }
    }
}
