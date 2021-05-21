// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class SyntaxNodeExtensions
    {
        public static bool IsNameOf( this InvocationExpressionSyntax node )
            => node.Expression.Kind() == SyntaxKind.NameOfKeyword ||
               (node.Expression is IdentifierNameSyntax identifierName && identifierName.Identifier.Text == "nameof");
    }
}