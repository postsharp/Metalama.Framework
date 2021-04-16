// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.Linq;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class ReflectionMapper
    {
        private readonly Compilation _compilation;
        private readonly ConcurrentDictionary<Type, ITypeSymbol> _symbolCache = new();
        private readonly ConcurrentDictionary<Type, NameSyntax> _syntaxCache = new();

        public ReflectionMapper( Compilation compilation )
        {
            this._compilation = compilation;
        }

        public INamedTypeSymbol GetTypeSymbolByReflectionName( string reflectionName )
        {
            var symbol = this._compilation.GetTypeByMetadataName( reflectionName );

            if ( symbol == null )
            {
                throw GeneralDiagnosticDescriptors.CannotFindType.CreateException( reflectionName );
            }

            return symbol;
        }

        private ITypeSymbol GetTypeSymbolCore( Type type )
        {
            if ( type is CompileTimeType compileTimeType )
            {
                return compileTimeType.TypeSymbol;
            }

            if ( type.IsByRef )
            {
                throw new ArgumentException( "Ref types cannot be represented as Caravela types." );
            }

            if ( type.IsArray )
            {
                var elementType = this.GetTypeSymbol( type.GetElementType()! );

                return this._compilation.CreateArrayTypeSymbol( elementType, type.GetArrayRank() );
            }

            if ( type.IsPointer )
            {
                var pointedToType = this.GetTypeSymbol( type.GetElementType()! );

                return this._compilation.CreatePointerTypeSymbol( pointedToType );
            }

            if ( type.IsConstructedGenericType )
            {
                var genericDefinition = this.GetTypeSymbolByReflectionName( type.GetGenericTypeDefinition().FullName );
                var genericArguments = type.GenericTypeArguments.Select( this.GetTypeSymbol ).ToArray();

                return genericDefinition.Construct( genericArguments! );
            }

            return this.GetTypeSymbolByReflectionName( type.FullName.AssertNotNull() );
        }

        public ITypeSymbol GetTypeSymbol( Type type ) => this._symbolCache.GetOrAdd( type, this.GetTypeSymbolCore );

        public NameSyntax GetTypeNameSyntax( Type type ) =>
            this._syntaxCache.GetOrAdd( type, t => (NameSyntax) CSharpSyntaxGenerator.Instance.NameExpression( this.GetTypeSymbol( t ) ) );
    }
}