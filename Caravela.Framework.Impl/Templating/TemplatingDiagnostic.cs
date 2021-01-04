using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Templating
{
    public static class TemplatingDiagnostic
    {
        public static Diagnostic CreateLanguageFeatureIsNotSupported( SyntaxNode node )
        {
            return Diagnostic.Create( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported, node.GetLocation(), GetSyntaxKindProperty( node.Kind() ), node.Kind().ToString() );
        }

        private static ImmutableDictionary<string, string?> GetSyntaxKindProperty( SyntaxKind syntaxKind )
        {
            return ImmutableDictionary<string, string?>.Empty.Add( TemplatingDiagnosticProperties.SyntaxKind, syntaxKind.ToString() );
        }
    }
}
