// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodBase = Metalama.Framework.Engine.CodeModel.MethodBase;

namespace Metalama.Framework.Engine.Serialization
{
    internal class CompileTimeFieldOrPropertyInfoSerializer : ObjectSerializer<CompileTimeFieldOrPropertyInfo, FieldOrPropertyInfo>
    {
        // TODO Add support for private indexers: currently, they're not found because we're only looking for public properties; we'd need to use the overload with both types and
        // binding flags for private indexers, and that overload is complicated.

        public override ExpressionSyntax Serialize( CompileTimeFieldOrPropertyInfo obj, SyntaxSerializationContext serializationContext )
        {
            ExpressionSyntax propertyInfo;
            var allBindingFlags = SyntaxUtility.CreateBindingFlags( serializationContext );

            switch ( obj.FieldOrProperty )
            {
                case IProperty property:
                    {
                        propertyInfo = this.Service.CompileTimePropertyInfoSerializer.SerializeProperty( property, serializationContext );

                        break;
                    }

                case Field field:
                    {
                        var typeCreation = this.Service.Serialize( CompileTimeType.Create( field.DeclaringType ), serializationContext );

                        propertyInfo = InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    typeCreation,
                                    IdentifierName( "GetField" ) ) )
                            .AddArgumentListArguments(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal( field.Name ) ) ),
                                Argument( allBindingFlags ) )
                            .NormalizeWhitespace();

                        break;
                    }

                default:
                    throw new NotImplementedException();
            }

            return ObjectCreationExpression( serializationContext.GetTypeSyntax( typeof(FieldOrPropertyInfo) ) )
                .AddArgumentListArguments( Argument( propertyInfo ) )
                .NormalizeWhitespace();
        }

        public CompileTimeFieldOrPropertyInfoSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof(MemberInfo), typeof(MethodBase) );
    }
}