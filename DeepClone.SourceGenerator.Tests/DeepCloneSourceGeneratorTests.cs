namespace DeepCloneSourceGenerator.Tests;

using System;
using System.Reflection;
using DeepClone.SourceGenerator;
using Microsoft.CodeAnalysis;
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
    public void Generator_Creates_Source_For_Real_Attribute(string attributeName)
    {
        string userCode = $$"""
            namespace DeepClone.SourceGenerator.UnitTests;
            [{{attributeName}}]
            public partial class MyCloneableClass { }
            """;

        TestHelper.RunGenerator<DeepCloneSourceGenerator>(userCode)
            .AssertGeneratedSourceExists("MyCloneableClass.g.cs");
    }

    [Theory]
    [InlineData("Description")]
    [InlineData("DescriptionAttribute")]
    [InlineData("System.ComponentModel.Description")]
    [InlineData("System.ComponentModel.DescriptionAttribute")]
    [InlineData("Fake.Namespace.DeepCloneable")]
    [InlineData("Fake.Namespace.DeepCloneableAttribute")]
    public void Generator_Doesnt_Create_Source_For_Other_Attribute(string attributeName)
    {
        string userCode = $$"""
            namespace DeepClone.SourceGenerator.UnitTests;
            [{{attributeName}}]
            public partial class MyCloneableClass { }
            """;

        TestHelper.RunGenerator<DeepCloneSourceGenerator>(userCode)
            .AssertGeneratedSourceDoesntExist("MyCloneableClass.g.cs");
    }

    [Fact]
    public void Recursive_Type_Matches_Snapshot()
    {
        string userCode = """
            namespace DeepClone.SourceGenerator.UnitTests;
            [DeepCloneable]
            public partial class MyRecursiveClass
            {
                public int MyInt { get; set; }
                public string MyString { get; set; }
                public MyRecursiveClass MyRecursiveClass { get; set; }
            }
            """;

        TestHelper.RunGenerator<DeepCloneSourceGenerator>(userCode)
            .AssertGeneratedSourceMatchesSnapshot("MyRecursiveClass.g.cs");
    }

    [Theory]
    [InlineData("MyRootClass.g.cs")]
    [InlineData("MyNestedClass.g.cs")]
    public void Nested_Type_Matches_Snapshot(string expectedHintName)
    {
        string userCode = """
            namespace DeepClone.SourceGenerator.UnitTests;
            [DeepCloneable]
            public partial class MyRootClass
            {
                public int Int { get; set; }
                public string String { get; set; }
                public MyNestedClass NestedClass { get; set; }
            }

            public partial class MyNestedClass
            {
                public int NestedInt { get; set; }
                public string NestedString { get; set; }
            }
            """;

        var result = TestHelper.RunGenerator<DeepCloneSourceGenerator>(userCode);

        result.AssertGeneratedSourceMatchesSnapshot(expectedHintName);
    }

    [Theory]
    [InlineData("Char.g.cs")]
    [InlineData("Int32.g.cs")]
    [InlineData("String.g.cs")]
    public void Generator_Doesnt_Generate_Source_For_Primitives(string primitiveHintName)
    {
        string userCode = """
            namespace DeepClone.SourceGenerator.UnitTests;
            [DeepCloneable]
            public partial class MyCloneableClass
            {
                public char MyChar { get; set; }
                public int MyInt { get; set; }
                public string MyString { get; set; }
            }
            """;

        TestHelper.RunGenerator<DeepCloneSourceGenerator>(userCode)
            .AssertGeneratedSourceDoesntExist(primitiveHintName);
    }

    [Fact]
    public void Generator_Matches_Type_Access_Modifier()
    {
        string userCode = """
            namespace DeepClone.SourceGenerator.UnitTests;
            [DeepCloneable]
            internal partial class MyInternalClass { }
            """;

        TestHelper.RunGenerator<DeepCloneSourceGenerator>(userCode)
            .AssertGeneratedSourceMatchesSnapshot("MyInternalClass.g.cs");
    }
}