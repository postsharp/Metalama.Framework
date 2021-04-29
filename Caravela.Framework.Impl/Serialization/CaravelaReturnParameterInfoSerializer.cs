// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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
            ExpressionSyntax? methodBaseExpression;
            switch ( o.DeclaringMember )
            {
                case Method method:
                    methodBaseExpression = this._methodInfoSerializer.Serialize( new CompileTimeMethodInfo( method ) );
                    break;
                
                default:
                    throw new NotImplementedException();
            }

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