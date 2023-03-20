namespace DeepClone.SourceGenerator.ExtensionMethods;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

internal static class AttributeListsExtensions
{
    public static bool ContainsAttribute(
        this SyntaxList<AttributeListSyntax> attributeLists,
        string attributeShortName) =>
        attributeLists.Any(
            x => x.Attributes.Any(
                y => AttributeNameMatches(y, attributeShortName)));

    private static bool AttributeNameMatches(AttributeSyntax attributeSyntax, string attributeShortName)
    {
        string? identifierText = attributeSyntax.Name switch
        {
            IdentifierNameSyntax identifierNameSyntax => identifierNameSyntax.Identifier.Text,
            QualifiedNameSyntax qualifiedNameSyntax => qualifiedNameSyntax.Right.Identifier.Text,
            _ => null
        };

        return identifierText is not null &&
               (identifierText.Equals(attributeShortName) ||
                identifierText.Equals($"{attributeShortName}Attribute"));
    }
}