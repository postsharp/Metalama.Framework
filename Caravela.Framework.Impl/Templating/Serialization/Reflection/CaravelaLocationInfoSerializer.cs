using Caravela.Compiler;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaLocationInfoSerializer : TypedObjectSerializer<CaravelaLocationInfo>
    {
        private readonly ObjectSerializers _serializers;

        public CaravelaLocationInfoSerializer( ObjectSerializers serializers ) => this._serializers = serializers;

        public override ExpressionSyntax Serialize( CaravelaLocationInfo o )
        {
            ExpressionSyntax propertyInfo;
            if ( o.Property != null )
            {
                var typeCreation = this._serializers.SerializeToRoslynCreationExpression( CaravelaType.Create( o.Property.DeclaringType ) );
                if ( o.Property.Parameters.Count == 0 )
                {
                    propertyInfo = InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                typeCreation,
                                IdentifierName( "GetProperty" ) ) )
                        .AddArgumentListArguments( Argument(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal( o.Property.Name ) ) ) );
                }
                else
                {
                    var returnTypeCreation = this._serializers.SerializeToRoslynCreationExpression( CaravelaType.Create( o.Property.Type ) );
                    List<ExpressionSyntax> parameterTypes = new List<ExpressionSyntax>();
                    foreach ( IParameter parameter in o.Property.Parameters )
                    {
                        parameterTypes.Add( this._serializers.SerializeToRoslynCreationExpression( CaravelaType.Create( parameter.Type ) ) );
                    }

                    propertyInfo = InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    typeCreation,
                                    IdentifierName( "GetProperty" ) ) )
                            .AddArgumentListArguments(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal( "Item" ) ) ),
                                Argument(
                                    returnTypeCreation ),
                                Argument(
                                    ArrayCreationExpression(
                                            ArrayType(
                                                    QualifiedName(
                                                        IdentifierName( "System" ),
                                                        IdentifierName( "Type" ) ) )
                                                .WithRankSpecifiers(
                                                    SingletonList(
                                                        ArrayRankSpecifier(
                                                            SingletonSeparatedList<ExpressionSyntax>(
                                                                OmittedArraySizeExpression() ) ) ) ) )
                                        .WithInitializer(
                                            InitializerExpression(
                                                SyntaxKind.ArrayInitializerExpression,
                                                SyntaxFactory.SeparatedList( parameterTypes ) ) ) )
                            )
                        ;
                }
            }
            else
            {
                Field f = o.Field!;
                string fieldCommentId = DocumentationCommentId.CreateDeclarationId( f.Symbol );
                string typeCommentId = DocumentationCommentId.CreateDeclarationId( ((f.ContainingElement as ITypeInternal)!).TypeSymbol );
                var containingType = IntrinsicsCaller.CreateLdTokenExpression( nameof(Intrinsics.GetRuntimeTypeHandle), typeCommentId );
                var fieldToken = IntrinsicsCaller.CreateLdTokenExpression( nameof(Caravela.Compiler.Intrinsics.GetRuntimeFieldHandle), fieldCommentId );
                propertyInfo = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName( "System" ),
                                    IdentifierName( "Reflection" ) ),
                                IdentifierName( "FieldInfo" ) ),
                            IdentifierName( "GetFieldFromHandle" ) ) )
                    .AddArgumentListArguments(
                        Argument( fieldToken ),
                        Argument( containingType ) )
                    .NormalizeWhitespace();
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
    }
}