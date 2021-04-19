// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CaravelaReturnParameterInfoSerializer : TypedObjectSerializer<CompileTimeReturnParameterInfo>
    {
        private readonly CaravelaMethodInfoSerializer _methodInfoSerializer;

        public CaravelaReturnParameterInfoSerializer( CaravelaMethodInfoSerializer methodInfoSerializer )
        {
            this._methodInfoSerializer = methodInfoSerializer;
        }

        public override ExpressionSyntax Serialize( CompileTimeReturnParameterInfo o )
        {
            var methodBaseExpression = this._methodInfoSerializer.Serialize( new CompileTimeMethodInfo( o.Method ) );

            return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ParenthesizedExpression(
                        CastExpression(
                            QualifiedName(
                                QualifiedName(
                                    IdentifierName( "System" ),
                                    IdentifierName( "Reflection" ) ),
                                IdentifierName( "MethodInfo" ) ),
                            methodBaseExpression ) ),
                    IdentifierName( "ReturnParameter" ) )
                .NormalizeWhitespace();
        }
    }
}