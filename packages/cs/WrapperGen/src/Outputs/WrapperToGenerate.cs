namespace WrapperGen.Outputs;

public enum DeclarationType
{
    Class,
    RecordClass,
    Record,

    Struct,
    RefStruct,
    ReadOnlyStruct,
    ReadOnlyRefStruct,

    RecordStruct,
    ReadOnlyRecordStruct,
}

public record struct WrapperToGenerate
{
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string ValuePropertyName { get; set; }
    public string ValuePropertyType { get; set; }
    public DeclarationType DeclarationType { get; set; }
    public bool FromValueConversion { get; set; }
    public bool ToValueConversion { get; set; }
    public bool GenerateValueProperty { get; set; }
    public bool GenerateConstructor { get; set; }
    public string? AssertValue { get; set; }
}