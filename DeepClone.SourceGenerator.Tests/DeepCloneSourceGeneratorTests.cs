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

        var result = RunGenerator<DeepCloneSourceGenerator>(userCode);

        GeneratedSourceResult? actual = result.Results.Single().GeneratedSources
            .FirstOrDefault(r => r.HintName == expectedHintName);

        Assert.Equal(expectedHintName, actual.Value.HintName);
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

        var result = RunGenerator<DeepCloneSourceGenerator>(userCode);

        GeneratedSourceResult? actual = result.Results.Single().GeneratedSources
            .FirstOrDefault(r => r.HintName == expectedHintName);

        Assert.NotEqual(expectedHintName, actual.Value.HintName);
    }

    private static GeneratorDriverRunResult RunGenerator<T>(string userCode)
        where T : IIncrementalGenerator, new()
    {
        var compilation = CSharpCompilation.Create("InMemoryUnitTestProject",
            new[] { CSharpSyntaxTree.ParseText(userCode) },
            Basic.Reference.Assemblies.Net70.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        IIncrementalGenerator generator = new T();
        ISourceGenerator sourceGenerator = generator.AsSourceGenerator();

        // trackIncrementalGeneratorSteps allows to report info about each step of the generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new ISourceGenerator[] { sourceGenerator },
            driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult();
    }
}