// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Serialization
{
    internal static class SyntaxUtility
    {
        public static ExpressionSyntax CreateBindingFlags( ISyntaxFactory syntaxFactory )
        {
            return new[] { "DeclaredOnly", "Public", "NonPublic", "Static", "Instance" }
                .Select(
                    f => (ExpressionSyntax) SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        syntaxFactory.GetTypeSyntax( typeof( BindingFlags ) ),
                        SyntaxFactory.IdentifierName( f ) ) )
                .Aggregate( ( l, r ) => SyntaxFactory.BinaryExpression( SyntaxKind.BitwiseOrExpression, l, r ) );
        }
    }
}