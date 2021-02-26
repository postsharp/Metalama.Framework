// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Templating
{
    internal static class TemplatingDiagnostic
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
