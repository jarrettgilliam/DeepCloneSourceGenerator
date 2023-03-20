namespace DeepClone.SourceGenerator.Interfaces;

internal interface ISourceCodeService
{
    string BaseNamespace { get; }
    string DeepCloneableAttributeDefinition { get; }
    string DeepCloneableAttributeFullName { get; }
    string DeepCloneableAttributeName { get; }
    string DeepCloneableAttributeShortName { get; }
    string GeneratedCodeAttributeUsage { get; }
}