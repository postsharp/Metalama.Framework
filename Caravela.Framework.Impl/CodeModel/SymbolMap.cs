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

        private readonly ConcurrentDictionary<ITypeSymbol, IType> _typeCache = new ();
        private readonly ConcurrentDictionary<IMethodSymbol, Method> _methodCache = new ();

        internal IType GetIType( ITypeSymbol typeSymbol ) => this._typeCache.GetOrAdd( typeSymbol, ts => CodeModelFactory.CreateIType( ts, this._compilation ) );

        internal NamedType GetNamedType( INamedTypeSymbol typeSymbol ) => (NamedType) this._typeCache.GetOrAdd( typeSymbol, ts => new NamedType( (INamedTypeSymbol) ts, this._compilation ) );

        internal GenericParameter GetGenericParameter( ITypeParameterSymbol typeSymbol ) =>
            (GenericParameter) this._typeCache.GetOrAdd( typeSymbol, ts => new GenericParameter( (ITypeParameterSymbol) ts, this._compilation ) );

        internal Method GetMethod( IMethodSymbol methodSymbol ) => this._methodCache.GetOrAdd( methodSymbol, ms => new Method( ms, this._compilation ) );

        internal CodeElement GetNamedTypeOrMethod( ISymbol symbol ) =>
            symbol switch
            {
                INamedTypeSymbol namedType => this.GetNamedType( namedType ),
                IMethodSymbol method => this.GetMethod( method ),
                _ => throw new ArgumentException( nameof( symbol ) )
            };
    }
}
