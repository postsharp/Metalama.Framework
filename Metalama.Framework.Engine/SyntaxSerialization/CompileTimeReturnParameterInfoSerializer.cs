// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal class CompileTimeReturnParameterInfoSerializer : ObjectSerializer<CompileTimeReturnParameterInfo, ParameterInfo>
    {
        public override ExpressionSyntax Serialize( CompileTimeReturnParameterInfo obj, SyntaxSerializationContext serializationContext )
        {
            var parameter = obj.Target.GetTarget( serializationContext.CompilationModel ).AssertNotNull();

            ExpressionSyntax? methodBaseExpression;

            switch ( parameter.DeclaringMember )
            {
                case IMethod method:
                    methodBaseExpression = this.Service.CompileTimeMethodInfoSerializer.Serialize(
                        CompileTimeMethodInfo.Create( method ),
                        serializationContext );

                    break;

                default:
                    throw new NotImplementedException();
            }

            return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    methodBaseExpression,
                    IdentifierName( "ReturnParameter" ) )
                .NormalizeWhitespace();
        }

        public CompileTimeReturnParameterInfoSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}