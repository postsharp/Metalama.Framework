// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.References
{
#pragma warning disable CA1822

    /// <summary>
    /// Builds instances of the <see cref="IRef{T}"/> interface.
    /// </summary>
    internal sealed class RefFactory
    {
        private readonly CompilationContext _compilationContext;

        public RefFactory( CompilationContext compilationContext )
        {
            this._compilationContext = compilationContext;
        }

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from an <see cref="IDeclarationBuilder"/>.
        /// </summary>
        public IRef<TCodeElement> FromBuilder<TCodeElement>( IDeclarationBuilder builder )
            where TCodeElement : class, IDeclaration
            => new BuilderRef<TCodeElement>( builder, this._compilationContext );

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from an <see cref="IDeclarationBuilder"/>.
        /// </summary>
        public IRef<IDeclaration> FromBuilder( IDeclarationBuilder builder )
            => builder.DeclarationKind switch
            {
                DeclarationKind.NamedType => new BuilderRef<INamedType>( builder, this._compilationContext ),
                DeclarationKind.Method => new BuilderRef<IMethod>( builder, this._compilationContext ),
                DeclarationKind.Property => new BuilderRef<IProperty>( builder, this._compilationContext ),
                DeclarationKind.Indexer => new BuilderRef<IIndexer>( builder, this._compilationContext ),
                DeclarationKind.Field => new BuilderRef<IFieldOrProperty>( builder, this._compilationContext ),
                DeclarationKind.Event => new BuilderRef<IEvent>( builder, this._compilationContext ),
                DeclarationKind.Parameter => new BuilderRef<IParameter>( builder, this._compilationContext ),
                DeclarationKind.TypeParameter => new BuilderRef<ITypeParameter>( builder, this._compilationContext ),
                DeclarationKind.Attribute => new BuilderRef<IAttribute>( builder, this._compilationContext ),
                DeclarationKind.ManagedResource => new BuilderRef<IManagedResource>( builder, this._compilationContext ),
                DeclarationKind.Constructor => new BuilderRef<IConstructor>( builder, this._compilationContext ),
                DeclarationKind.Finalizer => new BuilderRef<IMethod>( builder, this._compilationContext ),
                DeclarationKind.Operator => new BuilderRef<IMethod>( builder, this._compilationContext ),
                DeclarationKind.AssemblyReference => new BuilderRef<IAssembly>( builder, this._compilationContext ),
                DeclarationKind.Namespace => new BuilderRef<INamespace>( builder, this._compilationContext ),
                _ => throw new ArgumentOutOfRangeException()
            };

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from a Roslyn symbol.
        /// </summary>
        public IRef<IDeclaration> FromDeclarationSymbol( ISymbol symbol ) => (IRef<IDeclaration>) this.FromAnySymbol( symbol );

        public IRef<ICompilationElement> FromAnySymbol( ISymbol symbol )
            => symbol.GetDeclarationKind( this._compilationContext ) switch
            {
                DeclarationKind.Compilation => new SymbolRef<ICompilation>( symbol, this._compilationContext ),
                DeclarationKind.NamedType => new SymbolRef<INamedType>( symbol, this._compilationContext ),
                DeclarationKind.Method => new SymbolRef<IMethod>( symbol, this._compilationContext ),
                DeclarationKind.Property => new SymbolRef<IProperty>( symbol, this._compilationContext ),
                DeclarationKind.Indexer => new SymbolRef<IIndexer>( symbol, this._compilationContext ),
                DeclarationKind.Field => new SymbolRef<IFieldOrProperty>( symbol, this._compilationContext ),
                DeclarationKind.Event => new SymbolRef<IEvent>( symbol, this._compilationContext ),
                DeclarationKind.Parameter => new SymbolRef<IParameter>( symbol, this._compilationContext ),
                DeclarationKind.TypeParameter => new SymbolRef<ITypeParameter>( symbol, this._compilationContext ),
                DeclarationKind.Attribute => new SymbolRef<IAttribute>( symbol, this._compilationContext ),
                DeclarationKind.ManagedResource => new SymbolRef<IManagedResource>( symbol, this._compilationContext ),
                DeclarationKind.Constructor => new SymbolRef<IConstructor>( symbol, this._compilationContext ),
                DeclarationKind.Finalizer => new SymbolRef<IMethod>( symbol, this._compilationContext ),
                DeclarationKind.Operator => new SymbolRef<IMethod>( symbol, this._compilationContext ),
                DeclarationKind.AssemblyReference => new SymbolRef<IAssembly>( symbol, this._compilationContext ),
                DeclarationKind.Namespace => new SymbolRef<INamespace>( symbol, this._compilationContext ),
                DeclarationKind.Type => new SymbolRef<IType>( symbol, this._compilationContext ),
                _ => throw new ArgumentOutOfRangeException()
            };

        public IRef<IMethod> PseudoAccessor( IMethod accessor )
        {
            Invariant.Assert( accessor.IsImplicitlyDeclared );

            if ( accessor.ContainingDeclaration is not IHasAccessors declaringMember )
            {
                throw new AssertionFailedException( $"Unexpected containing declaration: '{accessor.ContainingDeclaration}'." );
            }

            return new SymbolRef<IMethod>(
                declaringMember.GetSymbol().AssertSymbolNotNull(),
                declaringMember.GetCompilationContext(),
                accessor.MethodKind.ToDeclarationRefTargetKind() );
        }

        public IRef<IParameter> PseudoParameter( IParameter pseudoParameter )
        {
            Invariant.Assert( pseudoParameter.GetCompilationContext() == this._compilationContext );

            var accessor = (IMethod) pseudoParameter.DeclaringMember;

            Invariant.Assert( accessor.IsImplicitlyDeclared );

            if ( accessor.ContainingDeclaration is not IHasAccessors declaringMember )
            {
                throw new AssertionFailedException( $"Unexpected containing declaration: '{accessor.ContainingDeclaration}'." );
            }

            return new SymbolRef<IParameter>(
                declaringMember.GetSymbol().AssertSymbolNotNull(),
                this._compilationContext,
                accessor.MethodKind switch
                {
                    MethodKind.PropertySet when pseudoParameter.IsReturnParameter => RefTargetKind.PropertySetReturnParameter,
                    MethodKind.PropertySet => RefTargetKind.PropertySetParameter,
                    MethodKind.PropertyGet => RefTargetKind.PropertyGetReturnParameter,
                    MethodKind.EventRaise when pseudoParameter.IsReturnParameter => RefTargetKind.EventRaiseReturnParameter,
                    MethodKind.EventRaise => throw new NotImplementedException(
                        $"Getting the reference of a pseudo event raiser parameter is not implemented." ),
                    _ => throw new AssertionFailedException( $"Unexpected MethodKind: {accessor.MethodKind}." )
                } );
        }

        public IRef<T> FromSymbolId<T>( SymbolId symbolKey )
            where T : class, ICompilationElement
            => new StringRef<T>( symbolKey.Id );

        public IRef<T> FromDeclarationId<T>( SerializableDeclarationId id )
            where T : class, ICompilationElement
            => new StringRef<T>( id.Id );

        public IRef<T> FromTypeId<T>( SerializableTypeId id )
            where T : class, IType
            => new StringRef<T>( id.Id );

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from a Roslyn symbol.
        /// </summary>
        public IRef<T> FromSymbol<T>(
            ISymbol symbol,
            RefTargetKind targetKind = RefTargetKind.Default )
            where T : class, ICompilationElement
            => new SymbolRef<T>( symbol, this._compilationContext, targetKind );

        public IRef<IParameter> ReturnParameter( IMethodSymbol methodSymbol )
            => new SymbolRef<IParameter>( methodSymbol, this._compilationContext, RefTargetKind.Return );

        internal IRef<ICompilation> Compilation( CompilationContext compilationContext )
        {
            Invariant.Assert( compilationContext == this._compilationContext );

            return this.FromSymbol<ICompilation>( compilationContext.Compilation.Assembly );
        }

        public IRef<T> FromSymbolBasedDeclaration<T>( SymbolBasedDeclaration declaration )
            where T : class, IDeclaration
        {
            Invariant.Assert( declaration.GetCompilationContext() == this._compilationContext );

            return this.FromSymbol<T>( declaration.Symbol );
        }
    }
}