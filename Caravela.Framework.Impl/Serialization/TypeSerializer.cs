// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class TypeSerializer : TypedObjectSerializer<Type>
    {
        public override ExpressionSyntax Serialize( Type o, ISyntaxFactory syntaxFactory ) 
            => this.SerializeTypeSymbolRecursive( syntaxFactory.GetTypeSymbol( o ), syntaxFactory );

        public ExpressionSyntax SerializeTypeSymbolRecursive( ITypeSymbol symbol, ISyntaxFactory syntaxFactory )
        {
            if ( symbol.TypeKind == TypeKind.Array )
            {
                var arraySymbol = (IArrayTypeSymbol) symbol;

                var makeArrayTypeArguments = arraySymbol.IsSZArray
                    ? new ArgumentSyntax[0]
                    : new[] { Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( arraySymbol.Rank ) ) ) };

                var innerTypeCreation = this.SerializeTypeSymbolRecursive( arraySymbol.ElementType, syntaxFactory );

                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            innerTypeCreation,
                            IdentifierName( "MakeArrayType" ) ) )
                    .AddArgumentListArguments( makeArrayTypeArguments )
                    .NormalizeWhitespace();
            }

            if ( symbol is ITypeParameterSymbol typeParameterSymbol )
            {
                ExpressionSyntax declaringExpression;

                if ( typeParameterSymbol.DeclaringMethod is { } method )
                {
                    declaringExpression = this.Service.CompileTimeMethodInfoSerializer.SerializeMethodBase(
                        method.OriginalDefinition,
                        method.ContainingType.TypeParameters.Any() ? method.ContainingType : null,
                        syntaxFactory );
                }
                else
                {
                    var type = typeParameterSymbol.DeclaringType!.OriginalDefinition;
                    declaringExpression = this.SerializeTypeSymbolRecursive( type, syntaxFactory );
                }

                // expr.GetGenericArguments()[ordinal]
                return ElementAccessExpression(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                declaringExpression,
                                IdentifierName( "GetGenericArguments" ) ) ) )
                    .AddArgumentListArguments( Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( typeParameterSymbol.Ordinal ) ) ) );
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
                        arguments.Add( this.SerializeTypeSymbolRecursive( typeSymbol, syntaxFactory ) );
                    }
                }

                return InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SerializeTypeFromSymbolLeaf( basicType, syntaxFactory ),
                            IdentifierName( "MakeGenericType" ) ) )
                    .AddArgumentListArguments( arguments.Select( arg => Argument( arg ) ).ToArray() )
                    .NormalizeWhitespace();
            }

            return SerializeTypeFromSymbolLeaf( symbol, syntaxFactory );
        }

        private static ExpressionSyntax SerializeTypeFromSymbolLeaf( ITypeSymbol typeSymbol, ISyntaxFactory syntaxFactory )
        {
            var documentationId = DocumentationCommentId.CreateDeclarationId( typeSymbol );
            var token = IntrinsicsCaller.CreateLdTokenExpression( nameof(Intrinsics.GetRuntimeTypeHandle), documentationId );

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        syntaxFactory.GetTypeSyntax( typeof(Type) ),
                        IdentifierName( "GetTypeFromHandle" ) ) )
                .AddArgumentListArguments( Argument( token ) )
                .NormalizeWhitespace();
        }

        public TypeSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}