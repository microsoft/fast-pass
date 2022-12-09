namespace FastPass.Models;

// these models are to help us serialize the otherwise unserializable objects from the text analytics sdk
public class Entity
{
    public string NormalizedText { get; set; } = default!;
    public string Text { get; set; } = default!;
    public double ConfidenceScore { get; set; } = default!;
    public int Offset { get; set; } = default!;
    public int Length { get; set; } = default!;
    public Assertion Assertion { get; set; } = default!;
    public List<Link> Links { get; set; } = default!;
    public string Category { get; set; } = default!;
}

public class Link
{
    public string Id { get; set; } = default!;
    public string DataSource { get; set; } = default!;
}


public class Assertion
{
    public string Conditionality { get; set; } = default!;
    public string Certainty { get; set; } = default!;
    public string Association { get; set; } = default!;
}