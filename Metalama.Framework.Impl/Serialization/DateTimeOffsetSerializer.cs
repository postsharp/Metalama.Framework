// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Impl.Serialization
{
    internal class DateTimeOffsetSerializer : ObjectSerializer<DateTimeOffset>
    {
        public override ExpressionSyntax Serialize( DateTimeOffset obj, SyntaxSerializationContext serializationContext )
        {
            var isoTime = obj.ToString( "o" );

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        serializationContext.GetTypeSyntax( typeof(DateTimeOffset) ),
                        IdentifierName( "Parse" ) ) )
                .AddArgumentListArguments(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal( isoTime ) ) ) )
                .NormalizeWhitespace();
        }

        public DateTimeOffsetSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}