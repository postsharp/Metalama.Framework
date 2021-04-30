// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class DateTimeSerializer : TypedObjectSerializer<DateTime>
    {
        public override ExpressionSyntax Serialize( DateTime o, ISyntaxFactory syntaxFactory )
        {
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        syntaxFactory.GetTypeSyntax( typeof(DateTime) ),
                        IdentifierName( "FromBinary" ) ) )
                .AddArgumentListArguments(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal( o.ToBinary() ) ) ) );
        }

        public DateTimeSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}