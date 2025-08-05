namespace OrdrMate.Schemas.CustomizationMetadata;

public class SingleChoiceMetadata : ICustomizationMetadata
{
    public required List<SingleSelectOption> Choices { get; set; }
}

public class SingleSelectOption
{
    public required string Value { get; set; }
    public bool IsDefault { get; set; } = false;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal DeltaPrice { get; set; }
}