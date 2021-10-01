// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class TypeSerializer : ObjectSerializer<Type>
    {
        public override ExpressionSyntax Serialize( Type obj, ICompilationElementFactory syntaxFactory )
            => SerializeTypeSymbolRecursive( syntaxFactory.GetTypeSymbol( obj ) );

        public static ExpressionSyntax SerializeTypeSymbolRecursive( ITypeSymbol symbol )
        {
            switch ( symbol )
            {
                case ITypeParameterSymbol:
                    // Serializing a generic parameter always assume that we are in a lexical scope where
                    // the symbol exists. Getting the generic parameter e.g. using typeof(X).GetGenericArguments()[Y]
                    // is not supported and would require an API change.
                    return TypeOfExpression( IdentifierName( symbol.Name ) );

                default:
                    return SerializeTypeFromSymbolLeaf( symbol );
            }
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