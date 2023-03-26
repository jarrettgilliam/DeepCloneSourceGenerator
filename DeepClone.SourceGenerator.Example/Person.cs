namespace DeepClone.SourceGenerator.Example;

using System;

[DeepCloneable]
internal sealed partial class Person
{
    public Name? Name { get; set; }
    public DateOnly Birthday { get; set; }
    public string SocialSecurityNumber { get; set; }

}