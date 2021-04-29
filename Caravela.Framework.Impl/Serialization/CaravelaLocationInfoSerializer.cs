// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CaravelaLocationInfoSerializer : TypedObjectSerializer<CompileTimeFieldOrPropertyInfo>
    {
        // TODO Add support for private indexers: currently, they're not found because we're only looking for public properties; we'd need to use the overload with both types and
        // binding flags for private indexers, and that overload is complicated.

        private readonly SyntaxSerializationService _serializers;

        public CaravelaLocationInfoSerializer( SyntaxSerializationService serializers )
        {
            this._serializers = serializers;
        }

        public override ExpressionSyntax Serialize( CompileTimeFieldOrPropertyInfo o )
        {
            ExpressionSyntax propertyInfo;
            var allBindingFlags = CreateBindingFlags();

            switch ( o.FieldOrProperty )
            {
                case IProperty property:
                    {
                        var typeCreation = this._serializers.Serialize( CompileTimeType.Create( property.DeclaringType ) );

                        if ( property.Parameters.Count == 0 )
                        {
                            propertyInfo = InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        typeCreation,
                                        IdentifierName( "GetProperty" ) ) )
                                .AddArgumentListArguments(
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal( property.Name ) ) ),
                                    Argument( allBindingFlags ) );
                        }
                        else
                        {
                            var returnTypeCreation = this._serializers.Serialize( CompileTimeType.Create( property.Type ) );
                            var parameterTypes = new List<ExpressionSyntax>();

                            foreach ( var parameter in property.Parameters )
                            {
                                parameterTypes.Add( this._serializers.Serialize( CompileTimeType.Create( parameter.ParameterType ) ) );
                            }

                            var propertyName = property.GetSymbol().AssertNotNull().MetadataName;

                            propertyInfo = InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            typeCreation,
                                            IdentifierName( "GetProperty" ) ) )
                                    .AddArgumentListArguments(
                                        Argument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal( propertyName ) ) ),
                                        Argument( returnTypeCreation ),
                                        Argument(
                                            ArrayCreationExpression(
                                                    ArrayType(
                                                            QualifiedName(
                                                                IdentifierName( "System" ),
                                                                IdentifierName( "Type" ) ) )
                                                        .WithRankSpecifiers(
                                                            SingletonList(
                                                                ArrayRankSpecifier(
                                                                    SingletonSeparatedList<ExpressionSyntax>( OmittedArraySizeExpression() ) ) ) ) )
                                                .WithInitializer(
                                                    InitializerExpression(
                                                        SyntaxKind.ArrayInitializerExpression,
                                                        SeparatedList( parameterTypes ) ) ) ) )
                                ;
                        }
                        
                        break;
                    }

                case Field field:
                    {
                        var typeCreation = this._serializers.Serialize( CompileTimeType.Create( field.DeclaringType ) );

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

            return ObjectCreationExpression(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName( "Caravela" ),
                            IdentifierName( "Framework" ) ),
                        IdentifierName( "LocationInfo" ) ) )
                .AddArgumentListArguments( Argument( propertyInfo ) )
                .NormalizeWhitespace();
        }

        private static ExpressionSyntax MemberAccess( params string[] names )
        {
            ExpressionSyntax result = IdentifierName( names[0] );

            for ( var i = 1; i < names.Length; i++ )
            {
                result = MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, result, IdentifierName( names[i] ) );
            }

            return result!;
        }

        private static ExpressionSyntax CreateBindingFlags()
        {
            return new[] { "DeclaredOnly", "Public", "NonPublic", "Static", "Instance" }
                .Select( f => MemberAccess( "System", "Reflection", "BindingFlags", f ) )
                .Aggregate( ( l, r ) => BinaryExpression( SyntaxKind.BitwiseOrExpression, l, r ) );
        }
    }
}