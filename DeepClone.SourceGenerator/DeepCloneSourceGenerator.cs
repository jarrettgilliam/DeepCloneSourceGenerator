namespace DeepClone.SourceGenerator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using DeepClone.SourceGenerator.ExtensionMethods;
using DeepClone.SourceGenerator.Interfaces;
using DeepClone.SourceGenerator.Models;
using DeepClone.SourceGenerator.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
internal class DeepCloneSourceGenerator : IIncrementalGenerator
{
    private ISourceCodeService SourceCode { get; } = new SourceCodeService();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO: Output roslyn error messages for types with the DeepCloneable attribute that are not partial

        context.RegisterPostInitializationOutput(this.GenerateDeepCloneableAttribute);

        IncrementalValuesProvider<DeepCloneTypeInfo> deepCloneTypesProvider =
            context.SyntaxProvider.CreateSyntaxProvider(
                    this.IsDeepCloneableType,
                    this.GetTypeInfo)
                .WhereNotNull()
                .WithComparer(SymbolEqualityComparer.Default)
                .Collect()
                .SelectMany(this.FindReferencedPartialTypes)
                .Select(this.ToDeepCloneTypeInfo)
                .WithComparer(EqualityComparer<DeepCloneTypeInfo>.Default);

        context.RegisterSourceOutput(deepCloneTypesProvider, this.GenerateDeepClone);
    }

    private void GenerateDeepCloneableAttribute(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource(
            $"{this.SourceCode.DeepCloneableAttributeName}.g.cs",
            this.SourceCode.DeepCloneableAttributeDefinition);
    }

    private bool IsDeepCloneableType(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return syntaxNode is TypeDeclarationSyntax typeSyntax &&
               typeSyntax.Modifiers.Any(SyntaxKind.PartialKeyword) &&
               !typeSyntax.Modifiers.Any(SyntaxKind.StaticKeyword) &&
               typeSyntax.AttributeLists.ContainsAttribute(
                   this.SourceCode.DeepCloneableAttributeShortName,
                   this.SourceCode.BaseNamespace,
                   cancellationToken);
    }

    private INamedTypeSymbol? GetTypeInfo(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.Node is TypeDeclarationSyntax typeSyntax &&
            context.SemanticModel.GetDeclaredSymbol(typeSyntax, cancellationToken) is { } typeSymbol)
        {
            return typeSymbol;
        }

        return null;
    }

    private IEnumerable<INamedTypeSymbol> FindReferencedPartialTypes(
        ImmutableArray<INamedTypeSymbol> typeSymbols, CancellationToken cancellationToken)
    {
        HashSet<INamedTypeSymbol> referencedPartialTypes = new(SymbolEqualityComparer.Default);

        foreach (INamedTypeSymbol typeSymbol in typeSymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.FindPartialTypesReferencedByProperties(typeSymbol, referencedPartialTypes, cancellationToken);
        }

        return referencedPartialTypes;
    }

    private void FindPartialTypesReferencedByProperties(
        INamedTypeSymbol typeSymbol,
        ISet<INamedTypeSymbol> namedTypeSymbols,
        CancellationToken cancellationToken)
    {
        // TODO: Do more filter checking
        if (!this.IsPartialType(typeSymbol) ||
            !namedTypeSymbols.Add(typeSymbol))
        {
            return;
        }

        foreach (IPropertySymbol propertySymbol in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (propertySymbol.Type is INamedTypeSymbol namedTypeSymbol)
            {
                this.FindPartialTypesReferencedByProperties(namedTypeSymbol, namedTypeSymbols, cancellationToken);
            }
        }
    }

    private bool IsPartialType(INamedTypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        return typeSymbol.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .Any(typeDeclaration => typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword));
    }

    private DeepCloneTypeInfo ToDeepCloneTypeInfo(INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return new DeepCloneTypeInfo(
            symbol.ContainingNamespace.ToDisplayString(),
            symbol.Name,
            symbol.DeclaredAccessibility,
            this.GetPropertyInfos(symbol, cancellationToken));
    }

    private IEnumerable<DeepClonePropertyInfo> GetPropertyInfos(
        INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // TODO: capture access modifiers
        return symbol.GetMembers()
            .OfType<IPropertySymbol>()
            // TODO: Filter static/read-only properties
            .Select(property =>
                new DeepClonePropertyInfo(
                    property.Name,
                    this.IsPartialType(property.Type as INamedTypeSymbol)));
    }

    private void GenerateDeepClone(SourceProductionContext context, DeepCloneTypeInfo typeInfo)
    {
        context.AddSource($"{typeInfo.Name}.g.cs",
            $$"""
            // <auto-generated/>
            namespace {{typeInfo.Namespace}};

            {{this.GetAccessibility(typeInfo)}}partial class {{typeInfo.Name}}
            {
                {{this.SourceCode.GeneratedCodeAttributeUsage}}
                public {{typeInfo.Name}} DeepClone() =>
                    new()
                    {{{this.GeneratePropertyInitializers(typeInfo.Properties)}}
                    };
            }
            """);
    }

    private string? GetAccessibility(DeepCloneTypeInfo typeInfo)
    {
        return typeInfo.AccessModifier switch
        {
            Accessibility.Public => "public ",
            Accessibility.Internal => "internal ",
            Accessibility.Protected => "protected ",
            Accessibility.ProtectedAndInternal => "protected internal ",
            Accessibility.ProtectedOrInternal => "protected internal ",
            _ => null
        };
    }

    private string GeneratePropertyInitializers(
        IEnumerable<DeepClonePropertyInfo> deepClonePropertyInfos)
    {
        string propertyInitializers = string.Join(
            $",{Environment.NewLine}            ",
            deepClonePropertyInfos.Select(
                property =>
                {
                    string? deepCloneInvoke = property.IsPartialClass ? "?.DeepClone()" : null;

                    return $"{property.PropertyName} = this.{property.PropertyName}{deepCloneInvoke}";
                }));

        if (!string.IsNullOrEmpty(propertyInitializers))
        {
            propertyInitializers = $"{Environment.NewLine}            {propertyInitializers}";
        }

        return propertyInitializers;
    }
}