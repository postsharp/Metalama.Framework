// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodBase = Caravela.Framework.Impl.CodeModel.MethodBase;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CompileTimeFieldOrPropertyInfoSerializer : ObjectSerializer<CompileTimeFieldOrPropertyInfo, FieldOrPropertyInfo>
    {
        // TODO Add support for private indexers: currently, they're not found because we're only looking for public properties; we'd need to use the overload with both types and
        // binding flags for private indexers, and that overload is complicated.

        public override ExpressionSyntax Serialize( CompileTimeFieldOrPropertyInfo obj, ISyntaxFactory syntaxFactory )
        {
            ExpressionSyntax propertyInfo;
            var allBindingFlags = SyntaxUtility.CreateBindingFlags( syntaxFactory );

            switch ( obj.FieldOrProperty )
            {
                case IProperty property:
                    {
                        propertyInfo = this.Service.CompileTimePropertyInfoSerializer.SerializeProperty( property, syntaxFactory );

                        break;
                    }

                case Field field:
                    {
                        var typeCreation = this.Service.Serialize( CompileTimeType.Create( field.DeclaringType ), syntaxFactory );

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

            return ObjectCreationExpression( syntaxFactory.GetTypeSyntax( typeof( FieldOrPropertyInfo ) ) )
                .AddArgumentListArguments( Argument( propertyInfo ) )
                .NormalizeWhitespace();
        }

        public CompileTimeFieldOrPropertyInfoSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof( MemberInfo ), typeof( MethodBase ) );
    }
}