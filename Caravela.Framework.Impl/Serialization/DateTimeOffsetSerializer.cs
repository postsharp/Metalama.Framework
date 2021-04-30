// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class DateTimeOffsetSerializer : TypedObjectSerializer<DateTimeOffset>
    {
        public override ExpressionSyntax Serialize( DateTimeOffset o, ISyntaxFactory syntaxFactory )
        {
            var isoTime = o.ToString( "o" );

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        syntaxFactory.GetTypeSyntax( typeof(DateTimeOffset) ),
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