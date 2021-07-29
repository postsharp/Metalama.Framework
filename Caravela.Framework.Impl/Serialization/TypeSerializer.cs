// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class TypeSerializer : ObjectSerializer<Type>
    {
        public override ExpressionSyntax Serialize( Type obj, ICompilationElementFactory syntaxFactory )
            => this.SerializeTypeSymbolRecursive( syntaxFactory.GetTypeSymbol( obj ), syntaxFactory );

        public ExpressionSyntax SerializeTypeSymbolRecursive( ITypeSymbol symbol, ICompilationElementFactory syntaxFactory )
        {
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

            return SerializeTypeFromSymbolLeaf( symbol );
        }

        private static ExpressionSyntax SerializeTypeFromSymbolLeaf( ITypeSymbol typeSymbol )
        {
            // We always use typeof, regardless of the type accessibility. This means that the type must be accessible from the calling
            // context, but this is a reasonable assumption.
            return LanguageServiceFactory.CSharpSyntaxGenerator.TypeOfExpression( typeSymbol );
        }

        public TypeSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}