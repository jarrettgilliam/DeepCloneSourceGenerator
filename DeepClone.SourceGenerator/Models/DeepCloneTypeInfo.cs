namespace DeepClone.SourceGenerator.Models;

using System.Collections.Generic;

internal record DeepCloneTypeInfo(
    string ClassNamespace,
    string ClassName,
    IEnumerable<DeepClonePropertyInfo> Properties)
{
    public string ClassNamespace { get; } = ClassNamespace;
    public string ClassName { get; } = ClassName;
    public IEnumerable<DeepClonePropertyInfo> Properties { get; } = Properties;
}