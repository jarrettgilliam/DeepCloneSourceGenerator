namespace DeepClone.SourceGenerator.ExtensionMethods;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

internal static class AttributeListsExtensions
{
    public static bool ContainsAttribute(
        this SyntaxList<AttributeListSyntax> attributeLists,
        string attributeShortName,
        string attributeNamespace) =>
        attributeLists.Any(
            x => x.Attributes.Any(
                y => AttributeNameMatches(y, attributeShortName, attributeNamespace)));

    private static bool AttributeNameMatches(
        AttributeSyntax attributeSyntax,
        string attributeShortName,
        string attributeNamespace)
    {
        string? identifierText = attributeSyntax.Name switch
        {
            IdentifierNameSyntax i => i.Identifier.Text,
            QualifiedNameSyntax q when q.Left.ToString().Equals(attributeNamespace) => q.Right.Identifier.Text,
            _ => null
        };

        return identifierText is not null &&
               (identifierText.Equals(attributeShortName) ||
                identifierText.Equals($"{attributeShortName}Attribute"));
    }
}