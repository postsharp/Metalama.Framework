// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Source
{
    internal readonly struct SymbolBasedGenericMap
    {
        private readonly ImmutableArray<ITypeSymbol> _typeArguments;

        public SymbolBasedGenericMap( ImmutableArray<ITypeSymbol> typeArguments )
        {
            this._typeArguments = typeArguments;
        }

        public static readonly SymbolBasedGenericMap Empty = new( ImmutableArray<ITypeSymbol>.Empty );

        public bool IsEmpty => this._typeArguments.IsDefaultOrEmpty;

        [return: NotNullIfNotNull( nameof(type) )]
        public T? SubstituteSymbol<T>( T? type, Compilation compilation )
            where T : class, ITypeSymbol
        {
            if ( type is null )
            {
                return null;
            }

            if ( this.IsEmpty )
            {
                return type;
            }
            else
            {
                var mapper = new Mapper( this, compilation );

                return (T) mapper.Visit( type )!;
            }
        }

        /// <summary>
        /// Gets a <see cref="SymbolBasedGenericMap"/> for use in the base type.
        /// </summary>
        /// <param name="typeArguments">Type arguments of the base type in the current type.</param>
        public SymbolBasedGenericMap SubstituteSymbols( ImmutableArray<ITypeSymbol> typeArguments, Compilation compilation )
        {
            if ( typeArguments.IsEmpty )
            {
                if ( typeArguments.IsDefaultOrEmpty )
                {
                    return this;
                }
                else
                {
                    return new SymbolBasedGenericMap( this._typeArguments );
                }
            }
            else
            {
                var mappedTypeArgumentsBuilder = ImmutableArray.CreateBuilder<ITypeSymbol>( this._typeArguments.Length );

                foreach ( var typeArgument in this._typeArguments )
                {
                    mappedTypeArgumentsBuilder.Add( this.SubstituteSymbol( typeArgument, compilation ) );
                }

                return new SymbolBasedGenericMap( mappedTypeArgumentsBuilder.MoveToImmutable() );
            }
        }

        private sealed class Mapper : SymbolVisitor<ITypeSymbol>
        {
            private readonly SymbolBasedGenericMap _genericMap;
            private readonly Compilation _compilation;

            public Mapper( in SymbolBasedGenericMap genericMap, Compilation compilation )
            {
                this._genericMap = genericMap;
                this._compilation = compilation;
            }

            public override ITypeSymbol DefaultVisit( ISymbol symbol ) => throw new AssertionFailedException( $"Visitor not implemented for {symbol.Kind}." );

            public override ITypeSymbol VisitArrayType( IArrayTypeSymbol symbol )
            {
                var mappedElementType = this.Visit( symbol.ElementType )!;

                if ( ReferenceEquals( mappedElementType, symbol.ElementType ) )
                {
                    return symbol;
                }
                else
                {
                    return this._compilation.CreateArrayTypeSymbol( mappedElementType, symbol.Rank, symbol.ElementNullableAnnotation );
                }
            }

            public override ITypeSymbol VisitNamedType( INamedTypeSymbol symbol )
            {
                if ( !symbol.IsGenericType )
                {
                    return symbol;
                }
                else
                {
                    var mappedArguments = ImmutableArray.CreateBuilder<ITypeSymbol>( symbol.TypeArguments.Length );
                    var hasChange = false;

                    foreach ( var argument in symbol.TypeArguments )
                    {
                        var mappedArgument = this.Visit( argument )!;
                        hasChange |= !ReferenceEquals( mappedArgument, argument );

                        mappedArguments.Add( mappedArgument );
                    }

                    if ( !hasChange )
                    {
                        return symbol;
                    }
                    else
                    {
                        return symbol.ConstructedFrom.Construct( mappedArguments.MoveToImmutable(), symbol.TypeArgumentNullableAnnotations );
                    }
                }
            }

            public override ITypeSymbol VisitPointerType( IPointerTypeSymbol symbol )
            {
                var mappedPointedAtType = this.Visit( symbol.PointedAtType )!;

                if ( ReferenceEquals( mappedPointedAtType, symbol.PointedAtType ) )
                {
                    return symbol;
                }
                else
                {
                    return this._compilation.CreatePointerTypeSymbol( mappedPointedAtType );
                }
            }

            public override ITypeSymbol VisitFunctionPointerType( IFunctionPointerTypeSymbol symbol ) => throw new NotImplementedException();

            public override ITypeSymbol VisitTypeParameter( ITypeParameterSymbol symbol )
                => symbol.DeclaringType != null ? this._genericMap._typeArguments[symbol.Ordinal] : symbol;
        }
    }
}