namespace DeepClone.SourceGenerator.Examples;

using System;
using DeepClone.SourceGenerator.Example;

[DeepCloneable]
public sealed partial class Person : ICloneable
{
    public Name? Name { get; set; }
    public object Clone() => this.DeepClone();
}