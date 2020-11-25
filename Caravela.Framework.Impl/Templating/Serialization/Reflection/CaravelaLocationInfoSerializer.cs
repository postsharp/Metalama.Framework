using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaLocationInfoSerializer : TypedObjectSerializer<CaravelaLocationInfo>
    {
        private readonly ObjectSerializers _serializers;

        public CaravelaLocationInfoSerializer( ObjectSerializers serializers )
        {
            this._serializers = serializers;
        }
        
        public override ExpressionSyntax Serialize( CaravelaLocationInfo o )
        {
            if ( o.Property != null )
            {
                ITypeInternal p = (o.Property.DeclaringType as ITypeInternal)!;
                var typeCreation = this._serializers.SerializeToRoslynCreationExpression( new CaravelaType( p.TypeSymbol ));
                ExpressionSyntax propertyInfo = null;
                if ( o.Property.Parameters.Count == 0 )
                {
                    propertyInfo = InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                typeCreation,
                                IdentifierName( "GetProperty" ) ) )
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList<ArgumentSyntax>(
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal( o.Property.Name ) ) ) ) ) );
                }
                else
                {
                    var returnTypeCreation = this._serializers.SerializeToRoslynCreationExpression( new CaravelaType((o.Property.Type as ITypeInternal).TypeSymbol ));
                    List<ExpressionSyntax> parameterTypes = new List<ExpressionSyntax>();
                    foreach ( IParameter parameter in o.Property.Parameters )
                    {
                        CaravelaType cType = new CaravelaType( (parameter.Type as ITypeInternal).TypeSymbol );
                        parameterTypes.Add( this._serializers.SerializeToRoslynCreationExpression( cType ) );
                    }

                    propertyInfo = InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    typeCreation,
                                    IdentifierName( "GetProperty" ) ) )
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList<ArgumentSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            Argument(
                                                LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    Literal( "Item" ) ) ),
                                            Token( SyntaxKind.CommaToken ), Argument(
                                                returnTypeCreation ),
                                            Token( SyntaxKind.CommaToken ), Argument(
                                                ArrayCreationExpression(
                                                        ArrayType(
                                                                QualifiedName(
                                                                    IdentifierName("System"),
                                                                    IdentifierName("Type")) )
                                                            .WithRankSpecifiers(
                                                                SingletonList<ArrayRankSpecifierSyntax>(
                                                                    ArrayRankSpecifier(
                                                                        SingletonSeparatedList<ExpressionSyntax>(
                                                                            OmittedArraySizeExpression() ) ) ) ) )
                                                    .WithInitializer(
                                                        InitializerExpression(
                                                            SyntaxKind.ArrayInitializerExpression,
                                                            InitializerFormer.CreateCommaSeparatedList( parameterTypes ) ) ) )
                                        } ) ) )
                        ;
                }

                return ObjectCreationExpression(
                        QualifiedName(
                            QualifiedName(
                                IdentifierName( "Caravela" ),
                                IdentifierName( "Framework" ) ),
                            IdentifierName( "LocationInfo" ) ) )
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList<ArgumentSyntax>(
                                Argument( propertyInfo ) ) ) )
                    .NormalizeWhitespace();
            }

            throw new Exception( "Fields not supported yet." );
        }
    }
}