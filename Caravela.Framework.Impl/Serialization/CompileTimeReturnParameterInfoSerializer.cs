// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CompileTimeReturnParameterInfoSerializer : ObjectSerializer<CompileTimeReturnParameterInfo, ParameterInfo>
    {
        public override ExpressionSyntax Serialize( CompileTimeReturnParameterInfo obj, ICompilationElementFactory syntaxFactory )
        {
            var parameter = obj.Target.Resolve( syntaxFactory.CompilationModel );

            ExpressionSyntax? methodBaseExpression;

            switch ( parameter.DeclaringMember )
            {
                case IMethod method:
                    methodBaseExpression = this.Service.CompileTimeMethodInfoSerializer.Serialize( CompileTimeMethodInfo.Create( method ), syntaxFactory );

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