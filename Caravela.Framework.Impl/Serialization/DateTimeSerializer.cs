// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class DateTimeSerializer : TypedObjectSerializer<DateTime>
    {
        public override ExpressionSyntax Serialize( DateTime o )
        {
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName( "System" ),
                            IdentifierName( "DateTime" ) ),
                        IdentifierName( "FromBinary" ) ) )
                .AddArgumentListArguments(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal( o.ToBinary() ) ) ) );
        }
    }
}