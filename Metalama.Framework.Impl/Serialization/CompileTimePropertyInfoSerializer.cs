// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Framework.Impl.Serialization
{
    internal class CompileTimePropertyInfoSerializer : ObjectSerializer<CompileTimePropertyInfo, PropertyInfo>
    {
        public CompileTimePropertyInfoSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ExpressionSyntax Serialize( CompileTimePropertyInfo obj, SyntaxSerializationContext serializationContext )
        {
            var property = obj.Target.GetTarget( serializationContext.CompilationModel ).AssertNotNull();

            return this.SerializeProperty( property, serializationContext );
        }

        public ExpressionSyntax SerializeProperty( IProperty property, SyntaxSerializationContext serializationContext )
        {
            var typeCreation = this.Service.Serialize( CompileTimeType.Create( property.DeclaringType ), serializationContext );

            if ( property.Parameters.Count == 0 )
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
                                SyntaxFactory.Literal( property.Name ) ) ),
                        SyntaxFactory.Argument( SyntaxUtility.CreateBindingFlags( serializationContext ) ) );
            }
            else
            {
                var returnTypeCreation = this.Service.Serialize( CompileTimeType.Create( property.Type ), serializationContext );
                var parameterTypes = new List<ExpressionSyntax>();

                foreach ( var parameter in property.Parameters )
                {
                    parameterTypes.Add( this.Service.Serialize( CompileTimeType.Create( parameter.Type ), serializationContext ) );
                }

                var propertyName = property.GetSymbol().AssertNotNull().MetadataName;

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
        }

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof(MemberInfo) );
    }
}