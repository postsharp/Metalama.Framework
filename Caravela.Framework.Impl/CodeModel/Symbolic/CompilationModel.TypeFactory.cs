// unset

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract partial class CompilationModel
    {
        private readonly ConcurrentDictionary<ITypeSymbol, IType> _typeCache = new();
        private readonly ConcurrentDictionary<IMethodSymbol, IMethod> _methodCache = new();

        public IType? GetTypeByReflectionType( Type type )
        {
            if ( type.IsByRef )
            {
                throw new ArgumentException( "Ref types can't be represented as Caravela types." );
            }

            if ( type.IsArray )
            {
                var elementType = this.GetTypeByReflectionType( type.GetElementType() );

                return elementType?.MakeArrayType( type.GetArrayRank() );
            }

            if ( type.IsPointer )
            {
                var pointedToType = this.GetTypeByReflectionType( type.GetElementType() );

                return pointedToType?.MakePointerType();
            }

            if ( type.IsConstructedGenericType )
            {
                var genericDefinition = this.GetTypeByReflectionName( type.GetGenericTypeDefinition().FullName );
                var genericArguments = type.GenericTypeArguments.Select( this.GetTypeByReflectionType ).ToArray();

                if ( genericArguments.Any( a => a == null ) )
                {
                    return null;
                }

                return genericDefinition?.MakeGenericType( genericArguments! );
            }

            return this.GetTypeByReflectionName( type.FullName );
        }
        internal IType GetIType( ITypeSymbol typeSymbol )
            => this._typeCache.GetOrAdd( typeSymbol, ts => CodeModelFactory.CreateIType( ts, this ) );

        protected abstract NamedType CreateNamedType( INamedTypeSymbol symbol );

        internal NamedType GetNamedType( INamedTypeSymbol typeSymbol )
            => (NamedType) this._typeCache.GetOrAdd( typeSymbol, s => this.CreateNamedType( (INamedTypeSymbol) s ) );

        internal GenericParameter GetGenericParameter( ITypeParameterSymbol typeParameterSymbol )
            => (GenericParameter) this._typeCache.GetOrAdd( typeParameterSymbol, new GenericParameter( typeParameterSymbol, this ) );

        internal IMethod GetMethod( IMethodSymbol methodSymbol )
            => this._methodCache.GetOrAdd( methodSymbol, ms => new Method( ms, this ) );

        internal ICodeElement GetNamedTypeOrMethod( ISymbol symbol ) =>
            symbol switch
            {
                INamedTypeSymbol namedType => this.GetNamedType( namedType ),
                IMethodSymbol method => this.GetMethod( method ),
                _ => throw new ArgumentException( nameof(symbol) )
            };

        IArrayType ITypeFactory.MakeArrayType( IType elementType, int rank ) =>
            (IArrayType) this.GetIType( this.RoslynCompilation.CreateArrayTypeSymbol( ((ITypeInternal) elementType).TypeSymbol, rank ) );

        IPointerType ITypeFactory.MakePointerType( IType pointedType ) =>
            (IPointerType) this.GetIType( this.RoslynCompilation.CreatePointerTypeSymbol( ((ITypeInternal) pointedType).TypeSymbol ) );

        bool ITypeFactory.Is( IType left, IType right ) => 
            this.RoslynCompilation.HasImplicitConversion( ((ITypeInternal) left).TypeSymbol, ((ITypeInternal) right).TypeSymbol );
        bool ITypeFactory.Is( IType left, Type right ) =>
            this.RoslynCompilation.HasImplicitConversion(
                ((ITypeInternal) left).TypeSymbol,
                ((ITypeInternal) this.GetTypeByReflectionType( right ))?.TypeSymbol ?? throw new ArgumentException( $"Could not resolve type {right}.", nameof( right ) ) );

    }
}