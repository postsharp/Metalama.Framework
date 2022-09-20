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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal class CompileTimePropertyInfoSerializer : ObjectSerializer<CompileTimePropertyInfo, PropertyInfo>
    {
        public CompileTimePropertyInfoSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ExpressionSyntax Serialize( CompileTimePropertyInfo obj, SyntaxSerializationContext serializationContext )
        {
            var property = obj.Target.GetTarget( serializationContext.CompilationModel ).AssertNotNull();

            return SerializeProperty( property, serializationContext );
        }

        public static ExpressionSyntax SerializeProperty( IPropertyOrIndexer propertyOrIndexer, SyntaxSerializationContext serializationContext )
        {
            var typeCreation = TypeSerializationHelper.SerializeTypeSymbolRecursive( propertyOrIndexer.DeclaringType.GetSymbol(), serializationContext );

            if ( propertyOrIndexer is IProperty )
            {
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            typeCreation,
                            IdentifierName( "GetProperty" ) ) )
                    .AddArgumentListArguments(
                        Argument(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal( propertyOrIndexer.Name ) ) ),
                        Argument( SyntaxUtility.CreateBindingFlags( propertyOrIndexer, serializationContext ) ) );
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

                return InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                typeCreation,
                                IdentifierName( "GetProperty" ) ) )
                        .AddArgumentListArguments(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal( propertyOrIndexer.Name ) ) ),
                            Argument( returnTypeCreation ),
                            Argument(
                                ArrayCreationExpression(
                                        ArrayType( serializationContext.GetTypeSyntax( typeof(Type) ) )
                                            .WithRankSpecifiers(
                                                SingletonList(
                                                    ArrayRankSpecifier( SingletonSeparatedList<ExpressionSyntax>( OmittedArraySizeExpression() ) ) ) ) )
                                    .WithInitializer(
                                        InitializerExpression(
                                            SyntaxKind.ArrayInitializerExpression,
                                            SeparatedList( parameterTypes ) ) ) ) )
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