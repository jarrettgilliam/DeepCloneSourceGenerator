namespace DeepCloneSourceGenerator.Tests;

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

internal static class TestHelper
{
    public static GeneratorDriverRunResult RunGenerator<T>(string userCode = "")
        where T : IIncrementalGenerator, new()
    {
        var compilation = GetCompilation(userCode);

        IIncrementalGenerator generator = new T();
        ISourceGenerator sourceGenerator = generator.AsSourceGenerator();

        // trackIncrementalGeneratorSteps allows to report info about each step of the generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: new ISourceGenerator[] { sourceGenerator },
            driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true));

        driver = driver.RunGenerators(compilation, CancellationToken.None);

        return driver.GetRunResult();
    }

    private static CSharpCompilation GetCompilation(string userCode, string[]? generatedCodeSnapshots = null)
    {
        var compilation = CSharpCompilation.Create(
            "InMemoryUnitTestProject",
            new[] { CSharpSyntaxTree.ParseText(userCode, cancellationToken: CancellationToken.None) },
            Basic.Reference.Assemblies.Net70.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        if (generatedCodeSnapshots != null)
        {
            foreach (string snapshot in generatedCodeSnapshots)
            {
                string generatedCode = GetGeneratedCodeSnapshot(snapshot);

                compilation = compilation.AddSyntaxTrees(
                    CSharpSyntaxTree.ParseText(generatedCode, cancellationToken: CancellationToken.None));
            }
        }

        return compilation;
    }

    public static void AssertGeneratedSourceMatchesSnapshot(this GeneratorDriverRunResult result, string expectedHintName)
    {
        string expectedGeneratedCode = GetGeneratedCodeSnapshot(expectedHintName);

        ImmutableArray<GeneratedSourceResult> generatedSources = result.Results.Single().GeneratedSources;

        Assert.Contains(expectedHintName, generatedSources.Select(x => x.HintName));

        var generatedSourceResult = generatedSources.First(x => x.HintName == expectedHintName);

        Assert.Equal(expectedGeneratedCode, generatedSourceResult.SourceText.ToString());
    }

    public static void AssertGeneratedSourceDoesntExist(this GeneratorDriverRunResult result, string expectedHintName)
    {
        ImmutableArray<GeneratedSourceResult> generatedSources = result.Results.Single().GeneratedSources;

        Assert.DoesNotContain(expectedHintName, generatedSources.Select(x => x.HintName));
    }

    public static void AssertGeneratedSourceExists(this GeneratorDriverRunResult result, string expectedHintName)
    {
        ImmutableArray<GeneratedSourceResult> generatedSources = result.Results.Single().GeneratedSources;

        Assert.Contains(expectedHintName, generatedSources.Select(x => x.HintName));
    }

    private static string GetGeneratedCodeSnapshot(string snapshotName) =>
        File.ReadAllText($"GeneratedCodeSnapshots/{snapshotName}").Replace("\r\n", "\n");
}