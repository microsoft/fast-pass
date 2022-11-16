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
        public List<object> Errors { get; set; }
        public TextAnalyticsTasks Tasks { get; set; }

        public class Assertion
        {
            public string Certainty { get; set; }
        }

        public class Author
        {
            public string Reference { get; set; }
            public string Type { get; set; }
            public string Display { get; set; }
        }

        public class Category
        {
            public List<Coding> Coding { get; set; }
            public string Text { get; set; }
        }

        public class Class
        {
            public string System { get; set; }
            public string Display { get; set; }
        }

        public class ClinicalStatus
        {
            public List<TextAnalyticsExtension> Extension { get; set; }
            public List<Coding> Coding { get; set; }
            public string Text { get; set; }
        }

        public class Code
        {
            public List<Coding> Coding { get; set; }
            public string Text { get; set; }
        }

        public class Coding
        {
            public string System { get; set; }
            public string Code { get; set; }
            public string Display { get; set; }
        }

        public class Context
        {
            public string Reference { get; set; }
            public string Type { get; set; }
            public string Display { get; set; }
        }

        public class Document
        {
            public string Id { get; set; }
            public List<Entity> Entities { get; set; }
            public List<Relation> Relations { get; set; }
            public List<object> Warnings { get; set; }
            public FhirBundle FhirBundle { get; set; }
        }

        public class EffectiveDateTime
        {
            public List<TextAnalyticsExtension> Extension { get; set; }
        }

        public class EffectiveTiming
        {
            public Code Code { get; set; }
        }

        public class Encounter
        {
            public string Reference { get; set; }
            public string Type { get; set; }
            public string Display { get; set; }
        }

        public class Entity
        {
            public int? Offset { get; set; }
            public int? Length { get; set; }
            public string Text { get; set; }
            public string Category { get; set; }
            public double? ConfidenceScore { get; set; }
            public string Name { get; set; }
            public List<Link> Links { get; set; }
            public Assertion Assertion { get; set; }
            public string Ref { get; set; }
            public string Role { get; set; }
        }

        public class Entry
        {
            public string FullUrl { get; set; }
            public Resource Resource { get; set; }
            public string Reference { get; set; }
            public string Type { get; set; }
            public string Display { get; set; }
            public Item Item { get; set; }
        }

        public class TextAnalyticsExtension
        {
            public List<TextAnalyticsExtension> Extension { get; set; }
            public string Url { get; set; }
            public int? ValueInteger { get; set; }
            public string ValueCode { get; set; }
        }

        public class FhirBundle
        {
            public string ResourceType { get; set; }
            public string Id { get; set; }
            public Meta Meta { get; set; }
            public Identifier Identifier { get; set; }
            public string Type { get; set; }
            public List<Entry> Entry { get; set; }
        }

        public class High
        {
            public int? Value { get; set; }
            public string Comparator { get; set; }
        }

        public class Identifier
        {
            public string System { get; set; }
            public string Value { get; set; }
        }

        public class Interpretation
        {
            public List<Coding> Coding { get; set; }
            public string Text { get; set; }
            public List<TextAnalyticsExtension> Extension { get; set; }
        }

        public class Item
        {
            public string Kind { get; set; }
            public DateTime? LastUpdateDateTime { get; set; }
            public string Status { get; set; }
            public Results Results { get; set; }
        }

        public class Item2
        {
            public string Reference { get; set; }
            public string Type { get; set; }
            public string Display { get; set; }
        }

        public class Link
        {
            public string DataSource { get; set; }
            public string Id { get; set; }
        }

        public class Low
        {
            public int? Value { get; set; }
            public string Comparator { get; set; }
        }

        public class MedicationCodeableConcept
        {
            public List<Coding> Coding { get; set; }
            public string Text { get; set; }
        }

        public class Meta
        {
            public List<string> Profile { get; set; }
        }

        public class Name
        {
            public string Text { get; set; }
            public string Family { get; set; }
            public List<string> Given { get; set; }
        }

        public class Note
        {
            public List<TextAnalyticsExtension> Extension { get; set; }
            public string Text { get; set; }
        }

        public class PerformedDateTime
        {
            public List<TextAnalyticsExtension> Extension { get; set; }
        }

        public class Period
        {
            public string Start { get; set; }
            public string End { get; set; }
        }

        public class Relation
        {
            public string RelationType { get; set; }
            public List<Entity> Entities { get; set; }
        }

        public class Resource
        {
            public string ResourceType { get; set; }
            public string Id { get; set; }
            public string Status { get; set; }
            public Type Type { get; set; }
            public Subject Subject { get; set; }
            public Encounter Encounter { get; set; }
            public string Date { get; set; }
            public List<Author> Author { get; set; }
            public string Title { get; set; }
            public List<Section> Section { get; set; }
            public List<Name> Name { get; set; }
            public List<TextAnalyticsExtension> Extension { get; set; }
            public string Gender { get; set; }
            public string BirthDate { get; set; }
            public Meta Meta { get; set; }
            public Class Class { get; set; }
            public Period Period { get; set; }
            public Code Code { get; set; }
            public EffectiveDateTime EffectiveDateTime { get; set; }
            public List<Interpretation> Interpretation { get; set; }
            public EffectiveTiming EffectiveTiming { get; set; }
            public ValueRange ValueRange { get; set; }
            public VerificationStatus VerificationStatus { get; set; }
            public List<Category> Category { get; set; }
            public string OnsetString { get; set; }
            public object BodySite { get; set; }
            public PerformedDateTime PerformedDateTime { get; set; }
            public ClinicalStatus ClinicalStatus { get; set; }
            public List<Note> Note { get; set; }
            public MedicationCodeableConcept MedicationCodeableConcept { get; set; }
            public Context Context { get; set; }
            public string Mode { get; set; }
            public List<Entry> Entry { get; set; }
        }

        public class Results
        {
            public List<Document> Documents { get; set; }
            public List<object> Errors { get; set; }
            public string ModelVersion { get; set; }
        }

        public class Section
        {
            public string Title { get; set; }
            public Code Code { get; set; }
            public Text Text { get; set; }
            public List<Entry> Entry { get; set; }
        }

        public class Subject
        {
            public string Reference { get; set; }
            public string Type { get; set; }
            public string Display { get; set; }
        }

        public class TextAnalyticsTasks
        {
            public int? Completed { get; set; }
            public int? Failed { get; set; }
            public int? InProgress { get; set; }
            public int? Total { get; set; }
            public List<Item> Items { get; set; }
        }

        public class Text
        {
            public string Status { get; set; }
            public string Div { get; set; }
        }

        public class Type
        {
            public List<Coding> Coding { get; set; }
            public string Text { get; set; }
        }

        public class ValueRange
        {
            public List<TextAnalyticsExtension> Extension { get; set; }
            public Low Low { get; set; }
            public High High { get; set; }
        }

        public class VerificationStatus
        {
            public List<Coding> Coding { get; set; }
            public string Text { get; set; }
        }



    }
}
