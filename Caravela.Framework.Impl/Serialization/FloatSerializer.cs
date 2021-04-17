// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization
{
    internal class FloatSerializer : TypedObjectSerializer<float>
    {
        public override ExpressionSyntax Serialize( float o )
        {
            if ( float.IsPositiveInfinity( o ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.FloatKeyword ) ),
                    SyntaxFactory.IdentifierName( "PositiveInfinity" ) );
            }

            if ( float.IsNegativeInfinity( o ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.FloatKeyword ) ),
                    SyntaxFactory.IdentifierName( "NegativeInfinity" ) );
            }

            if ( float.IsNaN( o ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.FloatKeyword ) ),
                    SyntaxFactory.IdentifierName( "NaN" ) );
            }

            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( o ) );
        }
    }
}