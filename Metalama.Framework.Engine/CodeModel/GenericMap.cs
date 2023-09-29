// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class GenericMap
    {
        private readonly Compilation _compilation;
        private readonly ImmutableArray<ITypeSymbol> _typeArguments;
        private Mapper? _mapper;

        public GenericMap( Compilation compilation ) : this( ImmutableArray<ITypeSymbol>.Empty, compilation ) { }

        public GenericMap( ImmutableArray<ITypeSymbol> typeArguments, Compilation compilation )
        {
            this._typeArguments = typeArguments;
            this._compilation = compilation;
        }

        [return: NotNullIfNotNull( nameof(type) )]
        public T? Map<T>( T? type )
            where T : class, ITypeSymbol
        {
            if ( type is null )
            {
                return null;
            }
            
            if ( this._typeArguments.IsEmpty )
            {
                return type;
            }
            else
            {
                this._mapper ??= new Mapper( this );

                return (T) this._mapper.Visit( type )!;
            }
        }

        /// <summary>
        /// Gets a <see cref="GenericMap"/> for use in the base type.
        /// </summary>
        /// <param name="typeArguments">Type arguments of the base type in the current type.</param>
        public GenericMap CreateBaseMap( ImmutableArray<ITypeSymbol> typeArguments )
        {
            if ( typeArguments.IsEmpty )
            {
                if ( this._typeArguments.IsEmpty )
                {
                    return this;
                }
                else
                {
                    return new GenericMap( this._typeArguments, this._compilation );
                }
            }
            else
            {
                var mappedTypeArgumentsBuilder = ImmutableArray.CreateBuilder<ITypeSymbol>( this._typeArguments.Length );

                foreach ( var typeArgument in this._typeArguments )
                {
                    mappedTypeArgumentsBuilder.Add( this.Map( typeArgument ) );
                }

                return new GenericMap( mappedTypeArgumentsBuilder.MoveToImmutable(), this._compilation );
            }
        }

        private sealed class Mapper : SymbolVisitor<ITypeSymbol>
        {
            private readonly GenericMap _parent;

            public Mapper( GenericMap parent )
            {
                this._parent = parent;
            }

            private Compilation Compilation => this._parent._compilation;

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
                    return this.Compilation.CreateArrayTypeSymbol( mappedElementType, symbol.Rank, symbol.ElementNullableAnnotation );
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
                    return this.Compilation.CreatePointerTypeSymbol( mappedPointedAtType );
                }
            }

            public override ITypeSymbol VisitFunctionPointerType( IFunctionPointerTypeSymbol symbol ) => throw new NotImplementedException();

            public override ITypeSymbol VisitTypeParameter( ITypeParameterSymbol symbol )
                => symbol.DeclaringType != null ? this._parent._typeArguments[symbol.Ordinal] : symbol;
        }
    }
}