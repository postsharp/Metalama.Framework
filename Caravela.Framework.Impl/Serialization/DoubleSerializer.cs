// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Serialization
{
    internal class DoubleSerializer : ObjectSerializer<double>
    {
        public override ExpressionSyntax Serialize( double obj, ISyntaxFactory syntaxFactory )
        {
            if ( double.IsPositiveInfinity( obj ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.DoubleKeyword ) ),
                    SyntaxFactory.IdentifierName( "PositiveInfinity" ) );
            }

            if ( double.IsNegativeInfinity( obj ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.DoubleKeyword ) ),
                    SyntaxFactory.IdentifierName( "NegativeInfinity" ) );
            }

            if ( double.IsNaN( obj ) )
            {
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType( SyntaxFactory.Token( SyntaxKind.DoubleKeyword ) ),
                    SyntaxFactory.IdentifierName( "NaN" ) );
            }

            return SyntaxFactory.LiteralExpression( SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal( obj ) );
        }

        public DoubleSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}