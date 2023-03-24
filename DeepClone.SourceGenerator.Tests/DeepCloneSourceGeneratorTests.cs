namespace DeepCloneSourceGenerator.Tests;

using System;
using System.Linq;
using System.Reflection;
using DeepClone.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

public class DeepCloneSourceGeneratorTests
{
    [Fact]
    public void Generator_Generates_DeepCloneableAttribute()
    {
        GeneratorDriverRunResult result = TestHelper.RunGenerator<DeepCloneSourceGenerator>();
        result.AssertGeneratedSourceMatchesSnapshot("DeepCloneableAttribute.g.cs");
    }

    [Fact]
    public void Generator_Has_GeneratorAttribute()
    {
        Type generatorType = typeof(DeepCloneSourceGenerator);

        GeneratorAttribute? attribute = generatorType.GetCustomAttribute<GeneratorAttribute>();

        Assert.NotNull(attribute);
        Assert.Collection(attribute!.Languages, r => Assert.Equal(LanguageNames.CSharp, r));
    }

    [Theory]
    [InlineData("DeepCloneable")]
    [InlineData("DeepCloneableAttribute")]
    [InlineData("DeepClone.SourceGenerator.DeepCloneable")]
    [InlineData("DeepClone.SourceGenerator.DeepCloneableAttribute")]
    public void Generator_Creates_Source_For_Type(string attributeName)
    {
        string userCode = $$"""
            namespace DeepClone.SourceGenerator.UnitTests;
            [{{attributeName}}]
            public partial class MyCloneableClass { }
            """;

        string expectedHintName = "MyCloneableClass.g.cs";

        var result = TestHelper.RunGenerator<DeepCloneSourceGenerator>(userCode);

        result.AssertGeneratedSourceExists(expectedHintName);
    }

    [Theory]
    [InlineData("Description")]
    [InlineData("DescriptionAttribute")]
    [InlineData("System.ComponentModel.Description")]
    [InlineData("System.ComponentModel.DescriptionAttribute")]
    [InlineData("Fake.Namespace.DeepCloneable")]
    [InlineData("Fake.Namespace.DeepCloneableAttribute")]
    public void Generator_Doesnt_Create_Source_For_Type(string attributeName)
    {
        string userCode = $$"""
            namespace DeepClone.SourceGenerator.UnitTests;
            [{{attributeName}}]
            public partial class MyCloneableClass { }
            """;

        string expectedHintName = "MyCloneableClass.g.cs";

        var result = TestHelper. RunGenerator<DeepCloneSourceGenerator>(userCode);

        result.AssertGeneratedSourceDoesntExist(expectedHintName);
    }

    [Fact]
    public void Generated_Source_Matches_Snapshot()
    {
        string userCode = """
            namespace DeepClone.SourceGenerator.UnitTests;
            [DeepCloneable]
            public partial class MyCloneableClass
            {
                public int MyInt { get; set; }
                public string MyString { get; set; }
                public MyCloneableClass MyCloneableClass { get; set; }
            }
            """;

        string expectedHintName = "MyCloneableClass.g.cs";

        var result = TestHelper. RunGenerator<DeepCloneSourceGenerator>(userCode);

        result.AssertGeneratedSourceMatchesSnapshot(expectedHintName);
    }
}