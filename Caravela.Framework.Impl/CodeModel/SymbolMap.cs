using System;
using System.Collections.Concurrent;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <remarks>
    /// Symbol map owns <see cref="IType"/> and <see cref="IMethod"/> objects in the compilation, other objects are owned by their container.
    /// </remarks>
    internal class SymbolMap
    {
        private readonly SourceCompilationModel _compilation;

        public SymbolMap( SourceCompilationModel compilation )
        {
            this._compilation = compilation;
        }

        private readonly ConcurrentDictionary<ITypeSymbol, ITypeInternal> _typeCache = new ( SymbolEqualityComparer.Default );
        
        private readonly ConcurrentDictionary<IMethodSymbol, Method> _methodCache = new ( SymbolEqualityComparer.Default );

        internal ITypeInternal GetIType( ITypeSymbol typeSymbol ) => this._typeCache.GetOrAdd( typeSymbol, ts => this.CreateIType( ts, this._compilation ) );

        internal NamedType GetNamedType( INamedTypeSymbol typeSymbol ) => (NamedType) this._typeCache.GetOrAdd( typeSymbol, ts => new SourceNamedType( this._compilation, ( INamedTypeSymbol) ts ) );

        internal GenericParameter GetGenericParameter( ITypeParameterSymbol typeSymbol ) =>
            (GenericParameter) this._typeCache.GetOrAdd( typeSymbol, ts => new SourceGenericParameter( this._compilation, ( ITypeParameterSymbol) ts ) );

        internal Method GetMethod( IMethodSymbol methodSymbol ) => this._methodCache.GetOrAdd( methodSymbol, ms => new SourceMethod( this._compilation, ms ) );

        internal CodeElement GetNamedTypeOrMethod( ISymbol symbol ) =>
            symbol switch
            {
                INamedTypeSymbol namedType => this.GetNamedType( namedType ),
                IMethodSymbol method => this.GetMethod( method ),
                _ => throw new ArgumentException( nameof( symbol ) )
            };
        
        private ITypeInternal CreateIType( ITypeSymbol typeSymbol, SourceCompilationModel compilation ) =>
            typeSymbol switch
            {
                INamedTypeSymbol namedType => new SourceNamedType( compilation, namedType ),
                IArrayTypeSymbol arrayType => new SourceArrayType( compilation, arrayType ),
                IPointerTypeSymbol pointerType => new SourcePointerType( compilation, pointerType ),
                ITypeParameterSymbol typeParameter => new SourceGenericParameter( compilation, typeParameter),
                IDynamicTypeSymbol dynamicType => new SourceDynamicType( compilation, dynamicType ),
                _ => throw new NotImplementedException()
            };
    }
}
