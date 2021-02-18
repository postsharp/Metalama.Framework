using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaTypeSerializer : TypedObjectSerializer<CaravelaType>
    {
        public override ExpressionSyntax Serialize( CaravelaType o )
        {
            return this.CreateTypeCreationExpressionFromSymbolRecursive( o.Symbol );
        }

        public ExpressionSyntax CreateTypeCreationExpressionFromSymbolRecursive( ITypeSymbol symbol )
        {
            if ( symbol.TypeKind == TypeKind.Array )
            {
                var arraySymbol = (IArrayTypeSymbol) symbol;
                var makeArrayTypeArguments = arraySymbol.IsSZArray
                    ? new ArgumentSyntax[0]
                    : new[] { Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( arraySymbol.Rank ) ) ) };
                var innerTypeCreation = this.CreateTypeCreationExpressionFromSymbolRecursive( arraySymbol.ElementType );
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            innerTypeCreation,
                            IdentifierName( "MakeArrayType" ) ) ).AddArgumentListArguments( makeArrayTypeArguments )
                    .NormalizeWhitespace();
            }

            if ( symbol is ITypeParameterSymbol typeParameterSymbol )
            {
                ExpressionSyntax declaringExpression;

                if ( typeParameterSymbol.DeclaringMethod is { } method )
                {
                    declaringExpression = CaravelaMethodInfoSerializer.CreateMethodBase( this, method.OriginalDefinition, method.ContainingType.TypeParameters.Any() ? method.ContainingType : null );
                }
                else
                {
                    var type = typeParameterSymbol.DeclaringType!.OriginalDefinition;
                    declaringExpression = this.CreateTypeCreationExpressionFromSymbolRecursive( type );
                }

                // expr.GetGenericArguments()[ordinal]
                return ElementAccessExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            declaringExpression,
                            IdentifierName( "GetGenericArguments" ) ) ) )
                    .AddArgumentListArguments(
                        Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( typeParameterSymbol.Ordinal ) ) ) );
            }

            if ( symbol is INamedTypeSymbol { IsGenericType: true, IsUnboundGenericType: false } namedSymbol &&
                !SymbolEqualityComparer.Default.Equals( namedSymbol.OriginalDefinition, namedSymbol ) )
            {
                var basicType = namedSymbol.ConstructUnboundGenericType();
                var arguments = new List<ExpressionSyntax>();
                var self = namedSymbol;
                var chain = new List<INamedTypeSymbol>();
                while ( self != null )
                {
                    chain.Add( self );
                    self = self.ContainingType;
                }

                chain.Reverse();
                foreach ( var layer in chain )
                {
                    foreach ( var typeSymbol in layer.TypeArguments )
                    {
                        arguments.Add( this.CreateTypeCreationExpressionFromSymbolRecursive( typeSymbol ) );
                    }
                }

                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateTypeCreationExpressionFromSymbolLeaf( basicType ),
                            IdentifierName( "MakeGenericType" ) ) )
                    .AddArgumentListArguments( arguments.Select( arg => Argument( arg ) ).ToArray() )
                    .NormalizeWhitespace();
            }

            return CreateTypeCreationExpressionFromSymbolLeaf( symbol );
        }

        private static ExpressionSyntax CreateTypeCreationExpressionFromSymbolLeaf( ITypeSymbol typeSymbol )
        {
            var documentationId = DocumentationCommentId.CreateDeclarationId( typeSymbol );
            var token = IntrinsicsCaller.CreateLdTokenExpression( nameof( Compiler.Intrinsics.GetRuntimeTypeHandle ), documentationId );
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName( "System" ),
                            IdentifierName( "Type" ) ),
                        IdentifierName( "GetTypeFromHandle" ) ) )
                .AddArgumentListArguments( Argument( token ) )
                .NormalizeWhitespace();
        }
    }
}