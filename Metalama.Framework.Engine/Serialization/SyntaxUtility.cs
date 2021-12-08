// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.Serialization
{
    internal static class SyntaxUtility
    {
        public static ExpressionSyntax CreateBindingFlags( SyntaxSerializationContext serializationContext )
        {
            return new[] { "DeclaredOnly", "Public", "NonPublic", "Static", "Instance" }
                .Select(
                    f => (ExpressionSyntax) SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        serializationContext.GetTypeSyntax( typeof(BindingFlags) ),
                        SyntaxFactory.IdentifierName( f ) ) )
                .Aggregate( ( l, r ) => SyntaxFactory.BinaryExpression( SyntaxKind.BitwiseOrExpression, l, r ) );
        }
    }
}