namespace DeepClone.SourceGenerator.Models;

internal record DeepClonePropertyInfo(
    string PropertyName)
{
    public string PropertyName { get; } = PropertyName;
}