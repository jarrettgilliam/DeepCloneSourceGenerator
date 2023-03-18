namespace DeepClone.SourceGenerator;

using DeepClone.SourceGenerator.Constants;
using Microsoft.CodeAnalysis;

[Generator]
internal class DeepCloneSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(AddDeepCloneableAttribute);
    }

    private static void AddDeepCloneableAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource(
            $"{nameof(SourceCode.DeepCloneableAttribute)}.g.cs",
            SourceCode.DeepCloneableAttribute);
    }
}