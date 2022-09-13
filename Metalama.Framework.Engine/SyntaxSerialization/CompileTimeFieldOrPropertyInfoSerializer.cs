// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal class CompileTimeFieldOrPropertyInfoSerializer : ObjectSerializer<CompileTimeFieldOrPropertyInfo, FieldOrPropertyInfo>
    {
        // TODO Add support for private indexers: currently, they're not found because we're only looking for public properties; we'd need to use the overload with both types and
        // binding flags for private indexers, and that overload is complicated.

        public override ExpressionSyntax Serialize( CompileTimeFieldOrPropertyInfo obj, SyntaxSerializationContext serializationContext )
        {
            ExpressionSyntax propertyInfo;
            var allBindingFlags = SyntaxUtility.CreateBindingFlags( obj.FieldOrProperty, serializationContext );

            switch ( obj.FieldOrProperty )
            {
                case IProperty property:
                    {
                        propertyInfo = CompileTimePropertyInfoSerializer.SerializeProperty( property, serializationContext );

                        break;
                    }

                case Field field:
                    {
                        var typeCreation = TypeSerializationHelper.SerializeTypeSymbolRecursive( field.DeclaringType.GetSymbol(), serializationContext );

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