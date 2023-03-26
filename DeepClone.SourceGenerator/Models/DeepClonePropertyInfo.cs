namespace DeepClone.SourceGenerator.Models;

internal record DeepClonePropertyInfo(
    string PropertyName,
    bool IsPartialClass)
{
    public string PropertyName { get; } = PropertyName;
    public bool IsPartialClass { get; } = IsPartialClass;
}