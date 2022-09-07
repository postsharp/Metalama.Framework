// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal class CompileTimePropertyInfoSerializer : ObjectSerializer<CompileTimePropertyInfo, PropertyInfo>
    {
        public CompileTimePropertyInfoSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ExpressionSyntax Serialize( CompileTimePropertyInfo obj, SyntaxSerializationContext serializationContext )
        {
            var property = obj.Target.GetTarget( serializationContext.CompilationModel ).AssertNotNull();

            return this.SerializeProperty( property, serializationContext );
        }

        public ExpressionSyntax SerializeProperty( IPropertyOrIndexer propertyOrIndexer, SyntaxSerializationContext serializationContext )
        {
            var typeCreation = TypeSerializationHelper.SerializeTypeSymbolRecursive( propertyOrIndexer.DeclaringType.GetSymbol(), serializationContext );

            if ( propertyOrIndexer is IProperty )
            {
                return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            typeCreation,
                            SyntaxFactory.IdentifierName( "GetProperty" ) ) )
                    .AddArgumentListArguments(
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal( propertyOrIndexer.Name ) ) ),
                        SyntaxFactory.Argument( SyntaxUtility.CreateBindingFlags( serializationContext ) ) );
            }
            else if ( propertyOrIndexer is IIndexer indexer )
            {
                var returnTypeCreation = TypeSerializationHelper.SerializeTypeSymbolRecursive( propertyOrIndexer.Type.GetSymbol(), serializationContext );
                var parameterTypes = new List<ExpressionSyntax>();

                foreach ( var parameter in indexer.Parameters )
                {
                    var parameterType = TypeSerializationHelper.SerializeTypeSymbolRecursive( parameter.Type.GetSymbol(), serializationContext );
                    parameterTypes.Add( parameterType );
                }

                var propertyName = propertyOrIndexer.GetSymbol().AssertNotNull().MetadataName;

                return SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                typeCreation,
                                SyntaxFactory.IdentifierName( "GetProperty" ) ) )
                        .AddArgumentListArguments(
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal( propertyName ) ) ),
                            SyntaxFactory.Argument( returnTypeCreation ),
                            SyntaxFactory.Argument(
                                SyntaxFactory.ArrayCreationExpression(
                                        SyntaxFactory.ArrayType( serializationContext.GetTypeSyntax( typeof(Type) ) )
                                            .WithRankSpecifiers(
                                                SyntaxFactory.SingletonList(
                                                    SyntaxFactory.ArrayRankSpecifier(
                                                        SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                            SyntaxFactory.OmittedArraySizeExpression() ) ) ) ) )
                                    .WithInitializer(
                                        SyntaxFactory.InitializerExpression(
                                            SyntaxKind.ArrayInitializerExpression,
                                            SyntaxFactory.SeparatedList( parameterTypes ) ) ) ) )
                    ;
            }
            else
            {
                throw new AssertionFailedException();
            }
        }

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof(MemberInfo) );
    }
}