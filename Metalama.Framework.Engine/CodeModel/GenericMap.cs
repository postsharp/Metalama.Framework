// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.CodeModel
{
    internal class GenericMap
    {
        private readonly Compilation _compilation;
        private Mapper? _mapper;

        public ImmutableArray<ITypeSymbol> TypeArguments { get; }

        public GenericMap( Compilation compilation ) : this( ImmutableArray<ITypeSymbol>.Empty, compilation ) { }

        private GenericMap( ImmutableArray<ITypeSymbol> typeArguments, Compilation compilation )
        {
            this.TypeArguments = typeArguments;
            this._compilation = compilation;
        }

        public ITypeSymbol Map( ITypeSymbol type )
        {
            if ( this.TypeArguments.IsEmpty )
            {
                return type;
            }
            else
            {
                this._mapper ??= new Mapper( this );

                return this._mapper.Visit( type )!;
            }
        }

        /// <summary>
        /// Gets a <see cref="GenericMap"/> for use in the base type.
        /// </summary>
        /// <param name="typeArguments">Type arguments of the base type in the current type.</param>
        /// <returns></returns>
        public GenericMap CreateBaseMap( ImmutableArray<ITypeSymbol> typeArguments )
        {
            if ( typeArguments.IsEmpty )
            {
                if ( this.TypeArguments.IsEmpty )
                {
                    return this;
                }
                else
                {
                    return new GenericMap( this.TypeArguments, this._compilation );
                }
            }
            else
            {
                var mappedTypeArgumentsBuilder = ImmutableArray.CreateBuilder<ITypeSymbol>( this.TypeArguments.Length );

                foreach ( var typeArgument in this.TypeArguments )
                {
                    mappedTypeArgumentsBuilder.Add( this.Map( typeArgument ) );
                }

                return new GenericMap( mappedTypeArgumentsBuilder.MoveToImmutable(), this._compilation );
            }
        }

        private class Mapper : SymbolVisitor<ITypeSymbol>
        {
            private readonly GenericMap _parent;

            public Mapper( GenericMap parent )
            {
                this._parent = parent;
            }

            private Compilation Compilation => this._parent._compilation;

            public override ITypeSymbol? DefaultVisit( ISymbol symbol ) => throw new AssertionFailedException();

            public override ITypeSymbol? VisitArrayType( IArrayTypeSymbol symbol )
            {
                var mappedElementType = symbol.ElementType;

                if ( ReferenceEquals( mappedElementType, symbol.ElementType ) )
                {
                    return symbol;
                }
                else
                {
                    return this.Compilation.CreateArrayTypeSymbol( mappedElementType, symbol.Rank, symbol.ElementNullableAnnotation );
                }
            }

            public override ITypeSymbol? VisitNamedType( INamedTypeSymbol symbol )
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

            public override ITypeSymbol? VisitPointerType( IPointerTypeSymbol symbol )
            {
                var mappedPointedAtType = this.Visit( symbol.PointedAtType )!;

                if ( ReferenceEquals( mappedPointedAtType, symbol.PointedAtType ) )
                {
                    return symbol;
                }
                else
                {
                    return this.Compilation.CreatePointerTypeSymbol( mappedPointedAtType );
                }
            }

            public override ITypeSymbol? VisitFunctionPointerType( IFunctionPointerTypeSymbol symbol ) => throw new NotImplementedException();

            public override ITypeSymbol? VisitTypeParameter( ITypeParameterSymbol symbol )
                => symbol.DeclaringType != null ? this._parent.TypeArguments[symbol.Ordinal] : symbol;
        }
    }
}