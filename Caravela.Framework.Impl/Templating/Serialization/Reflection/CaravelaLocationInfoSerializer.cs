using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaLocationInfoSerializer : TypedObjectSerializer<CaravelaLocationInfo>
    {
        // TODO Add support for private indexers: currently, they're not found because we're only looking for public properties; we'd need to use the overload with both types and
        // binding flags for private indexers, and that overload is complicated.
        
        private readonly ObjectSerializers _serializers;

        public CaravelaLocationInfoSerializer( ObjectSerializers serializers )
        {
            this._serializers = serializers;
        }

        public override ExpressionSyntax Serialize( CaravelaLocationInfo o )
        {
            ExpressionSyntax propertyInfo;
            var allBindingFlags = CreateBindingFlags();
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
                        .AddArgumentListArguments(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal( o.Property.Name ) ) ),
                            Argument( allBindingFlags ) );
                }
                else
                {
                    var returnTypeCreation = this._serializers.SerializeToRoslynCreationExpression( CaravelaType.Create( o.Property.Type ) );
                    var parameterTypes = new List<ExpressionSyntax>();
                    foreach ( var parameter in o.Property.Parameters )
                    {
                        parameterTypes.Add( this._serializers.SerializeToRoslynCreationExpression( CaravelaType.Create( parameter.Type ) ) );
                    }

                    var propertyName = o.Property.Symbol.MetadataName;

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
                                                SeparatedList( parameterTypes ) ) ) )
                            )
                        ;
                }
            }
            else
            {
                var typeCreation = this._serializers.SerializeToRoslynCreationExpression( CaravelaType.Create( o.Field!.DeclaringType ) );
                propertyInfo = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            typeCreation,
                            IdentifierName( "GetField" ) ) )
                    .AddArgumentListArguments(
                        Argument(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal( o.Field.Name ) ) ),
                        Argument( allBindingFlags )
                    )
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

        static ExpressionSyntax MemberAccess(params string[] names)
        {
            ExpressionSyntax result = IdentifierName(names[0]);
            for (var i = 1; i < names.Length; i++)
            {
                result = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, result, IdentifierName(names[i]));
            }
            return result!;
        }


        private static ExpressionSyntax CreateBindingFlags()
        {
            return new[] { "DeclaredOnly", "Public", "NonPublic", "Static", "Instance"}.Select( f => MemberAccess( "System", "Reflection", "BindingFlags", f ) )
                .Aggregate( ( l, r ) => BinaryExpression( SyntaxKind.BitwiseOrExpression, l, r ) );
        }
    }
}

