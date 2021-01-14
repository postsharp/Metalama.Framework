using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
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
                IArrayTypeSymbol arraySymbol = (symbol as IArrayTypeSymbol)!;
                ExpressionSyntax innerTypeCreation = this.CreateTypeCreationExpressionFromSymbolRecursive( arraySymbol.ElementType );
                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            innerTypeCreation,
                            IdentifierName( "MakeArrayType" ) ) )
                    .NormalizeWhitespace();
            }

            if ( symbol is INamedTypeSymbol {IsGenericType: true, IsUnboundGenericType: false} namedSymbol )
            {
                var basicType = namedSymbol.ConstructUnboundGenericType();
                List<ExpressionSyntax> arguments = new List<ExpressionSyntax>();
                bool hasTypeParameterSymbols = false;
                INamedTypeSymbol self = namedSymbol;
                List<INamedTypeSymbol> chain = new List<INamedTypeSymbol>();
                while ( self != null )
                {
                    chain.Add( self );
                    self = self.ContainingType;
                }

                chain.Reverse();
                foreach ( INamedTypeSymbol layer in chain )
                {


                    foreach ( ITypeSymbol typeSymbol in layer.TypeArguments )
                    {
                        if ( typeSymbol is ITypeParameterSymbol )
                        {
                            if ( arguments.Count > 0 )
                            {
                                throw new CaravelaException( GeneralDiagnosticDescriptors.TypeNotSerializable, layer );
                            }

                            hasTypeParameterSymbols = true;
                        }
                        else
                        {
                            if ( hasTypeParameterSymbols )
                            {
                                throw new CaravelaException( GeneralDiagnosticDescriptors.TypeNotSerializable, layer );
                            }

                            arguments.Add( this.CreateTypeCreationExpressionFromSymbolRecursive( typeSymbol ) );
                        }
                    }
                }

                if ( hasTypeParameterSymbols )
                {
                    return CreateTypeCreationExpressionFromSymbolLeaf( basicType );
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
            string documentationId = DocumentationCommentId.CreateDeclarationId( typeSymbol );
            var token = IntrinsicsCaller.CreateLdTokenExpression( nameof(Compiler.Intrinsics.GetRuntimeTypeHandle), documentationId );
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