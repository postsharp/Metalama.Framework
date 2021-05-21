// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Reflection;
using MethodBase = System.Reflection.MethodBase;

namespace Caravela.Framework.Impl.Serialization
{
    internal abstract class CaravelaMethodBaseSerializer<TInput, TOutput> : ObjectSerializer<TInput, TOutput>
        where TInput : MethodBase, TOutput
        where TOutput : MethodBase
    {
        public CaravelaMethodBaseSerializer( SyntaxSerializationService service ) : base( service ) { }

        internal ExpressionSyntax SerializeMethodBase( ICompileTimeReflectionObject<IMethodBase> method, ICompilationElementFactory syntaxFactory )
            => SerializeMethodBase( (IMethodSymbol) method.Target.GetSymbol( syntaxFactory.Compilation ), syntaxFactory );
            
        
        internal ExpressionSyntax SerializeMethodBase( IMethodSymbol methodSymbol, ICompilationElementFactory syntaxFactory )
        {
            return this.SerializeMethodBase( methodSymbol, methodSymbol.ContainingType, syntaxFactory );
        }

        internal ExpressionSyntax SerializeMethodBase(
            IMethodSymbol methodSymbol,
            ITypeSymbol? declaringGenericTypeSymbol,
            ICompilationElementFactory syntaxFactory )
        {
            methodSymbol = methodSymbol.OriginalDefinition;
            var documentationId = DocumentationCommentId.CreateDeclarationId( methodSymbol );
            var methodToken = IntrinsicsCaller.CreateLdTokenExpression( nameof(Intrinsics.GetRuntimeMethodHandle), documentationId );

            if ( declaringGenericTypeSymbol is INamedType namedtype && namedtype.GenericParameters.Count > 0 )
            {
                var typeHandle = this.CreateTypeHandleExpression( declaringGenericTypeSymbol, syntaxFactory );

                return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            syntaxFactory.GetTypeSyntax( typeof(MethodBase) ),
                            SyntaxFactory.IdentifierName( "GetMethodFromHandle" ) ) )
                    .AddArgumentListArguments( SyntaxFactory.Argument( methodToken ), SyntaxFactory.Argument( typeHandle ) )
                    .NormalizeWhitespace();
            }

            return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        syntaxFactory.GetTypeSyntax( typeof(MethodBase) ),
                        SyntaxFactory.IdentifierName( "GetMethodFromHandle" ) ) )
                .AddArgumentListArguments( SyntaxFactory.Argument( methodToken ) )
                .NormalizeWhitespace();
        }

        private ExpressionSyntax CreateTypeHandleExpression( ITypeSymbol type, ICompilationElementFactory syntaxFactory )
        {
            var typeExpression = this.Service.TypeSerializer.SerializeTypeSymbolRecursive( type, syntaxFactory );

            ExpressionSyntax typeHandle = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeExpression,
                SyntaxFactory.IdentifierName( "TypeHandle" ) );

            return typeHandle;
        }

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof(MemberInfo), typeof(MethodBase) );
    }
}