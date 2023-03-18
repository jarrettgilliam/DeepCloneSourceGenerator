namespace DeepClone.SourceGenerator.Models;

internal record DeepClonePropertyInfo(
    string PropertyName,
    bool IsValueTypeOrImmutable)
{
    public string PropertyName { get; } = PropertyName;
    public bool IsValueTypeOrImmutable { get; } = IsValueTypeOrImmutable;
}