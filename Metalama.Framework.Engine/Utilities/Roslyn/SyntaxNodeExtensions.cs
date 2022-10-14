// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    internal static class SyntaxNodeExtensions
    {
        public static bool IsNameOf( this InvocationExpressionSyntax node )
            => node.Expression.Kind() == SyntaxKind.NameOfKeyword ||
               (node.Expression is IdentifierNameSyntax identifierName && string.Equals( identifierName.Identifier.Text, "nameof", StringComparison.Ordinal ));
    }
}