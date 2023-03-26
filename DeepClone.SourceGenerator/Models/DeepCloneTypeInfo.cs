namespace DeepClone.SourceGenerator.Models;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

internal record DeepCloneTypeInfo(
    string Namespace,
    string Name,
    Accessibility AccessModifier,
    IEnumerable<DeepClonePropertyInfo> Properties)
{
    public string Namespace { get; } = Namespace;
    public string Name { get; } = Name;
    public Accessibility AccessModifier { get; } = AccessModifier;
    public IEnumerable<DeepClonePropertyInfo> Properties { get; } = Properties;
}