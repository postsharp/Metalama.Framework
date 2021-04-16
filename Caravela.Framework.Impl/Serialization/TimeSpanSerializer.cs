// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class TimeSpanSerializer : TypedObjectSerializer<TimeSpan>
    {
        public override ExpressionSyntax Serialize( TimeSpan o )
        {
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName( "System" ),
                        IdentifierName( "TimeSpan" ) ) )
                .AddArgumentListArguments(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal( o.Ticks ) ) ) )
                .NormalizeWhitespace();
        }
    }
}