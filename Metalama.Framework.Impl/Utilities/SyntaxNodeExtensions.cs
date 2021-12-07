// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class SyntaxNodeExtensions
    {
        public static bool IsNameOf( this InvocationExpressionSyntax node )
            => node.Expression.Kind() == SyntaxKind.NameOfKeyword ||
               (node.Expression is IdentifierNameSyntax identifierName && string.Equals( identifierName.Identifier.Text, "nameof", StringComparison.Ordinal ));

        public static string GetNameOfValue( this InvocationExpressionSyntax node )
            => node.ArgumentList.Arguments[0].Expression switch
            {
                // TODO: This may be incorrect when using with 'using alias = xxx'.

                SimpleNameSyntax simpleName => simpleName.Identifier.Text,
                QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.Text,
                _ => throw new NotImplementedException()
            };
    }
}