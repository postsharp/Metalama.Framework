// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.GenericContexts;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;
using Metalama.Framework.Engine.CodeModel.Introductions.Introduced;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// Builds instances of the <see cref="IRef{T}"/> interface.
    /// </summary>
    internal sealed partial class RefFactory
    {
        private readonly CompilationModel? _canonicalCompilationModel;
        private readonly ConcurrentDictionary<SymbolCacheKey, ISymbolRef<ICompilationElement>> _symbolCache = new( new SymbolCacheKeyComparer() );

        // There is no need for a cache of builder-based references because the unique instance of the reference is stored
        // inside DeclarationBuilderData.

        public CompilationContext CompilationContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefFactory"/> class and assigns a <see cref="CompilationModel"/>.
        /// </summary>
        public RefFactory( CompilationModel canonicalCompilationModel )
        {
            this._canonicalCompilationModel = canonicalCompilationModel;
            this.CompilationContext = canonicalCompilationModel.CompilationContext;
        }

        public CompilationModel CanonicalCompilation
            => this._canonicalCompilationModel ?? throw new InvalidOperationException( "The CanonicalCompilation is not available." );

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from an <see cref="IDeclarationBuilder"/>.
        /// </summary>
        public FullRef<T> FromBuilderData<T>( DeclarationBuilderData builder, GenericContext? genericContext = null )
            where T : class, IDeclaration
            => new IntroducedRef<T>( builder, this, genericContext );

        public FullRef<T> FromBuilt<T>( IntroducedDeclaration introducedDeclaration )
            where T : class, IDeclaration
            => this.FromBuilderData<T>( introducedDeclaration.BuilderData, introducedDeclaration.GenericContext );

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from a Roslyn symbol.
        /// </summary>
        public ISymbolRef<IDeclaration> FromDeclarationSymbol( ISymbol symbol ) => (ISymbolRef<IDeclaration>) this.FromAnySymbol( symbol );

        // Must be called _before_ cache lookup to make sure we have unique ref instances.
        private static ISymbol GetCanonicalSymbol( ISymbol symbol )
            => symbol.Kind switch
            {
                SymbolKind.Method => ((IMethodSymbol) symbol).PartialImplementationPart ?? symbol,
                _ => symbol
            };

        public ISymbolRef<ICompilationElement> FromAnySymbol( ISymbol symbol )
            => this._symbolCache.GetOrAdd(
                new SymbolCacheKey( GetCanonicalSymbol( symbol ), RefTargetKind.Default ),
                static ( key, me ) => key.Symbol.GetDeclarationKind( me.CompilationContext ) switch
                {
                    DeclarationKind.Compilation => new SymbolRef<ICompilation>( key.Symbol, me ),
                    DeclarationKind.NamedType => new SymbolRef<INamedType>( key.Symbol, me ),
                    DeclarationKind.Method => new SymbolRef<IMethod>( key.Symbol, me ),
                    DeclarationKind.Property => new SymbolRef<IProperty>( key.Symbol, me ),
                    DeclarationKind.Indexer => new SymbolRef<IIndexer>( key.Symbol, me ),
                    DeclarationKind.Field => new SymbolRef<IField>( key.Symbol, me ),
                    DeclarationKind.Event => new SymbolRef<IEvent>( key.Symbol, me ),
                    DeclarationKind.Parameter => new SymbolRef<IParameter>( key.Symbol, me ),
                    DeclarationKind.TypeParameter => new SymbolRef<ITypeParameter>( key.Symbol, me ),
                    DeclarationKind.Attribute => new SymbolRef<IAttribute>( key.Symbol, me ),
                    DeclarationKind.ManagedResource => new SymbolRef<IManagedResource>( key.Symbol, me ),
                    DeclarationKind.Constructor => new SymbolRef<IConstructor>( key.Symbol, me ),
                    DeclarationKind.Finalizer => new SymbolRef<IMethod>( key.Symbol, me ),
                    DeclarationKind.Operator => new SymbolRef<IMethod>( key.Symbol, me ),
                    DeclarationKind.AssemblyReference => new SymbolRef<IAssembly>( key.Symbol, me ),
                    DeclarationKind.Namespace => new SymbolRef<INamespace>( key.Symbol, me ),
                    DeclarationKind.Type => new SymbolRef<IType>( key.Symbol, me ),
                    _ => throw new ArgumentOutOfRangeException()
                },
                this );

        public SymbolRef<IMethod> PseudoAccessor( IMethod accessor )
        {
            Invariant.Assert( accessor.IsImplicitlyDeclared );
            Invariant.Assert( accessor.GetRefFactory() == this );

            if ( accessor.ContainingDeclaration is not IHasAccessors declaringMember )
            {
                throw new AssertionFailedException( $"Unexpected containing declaration: '{accessor.ContainingDeclaration}'." );
            }

            return new SymbolRef<IMethod>(
                declaringMember.GetSymbol().AssertSymbolNotNull(),
                this,
                accessor.MethodKind.ToDeclarationRefTargetKind() );
        }

        public SymbolRef<IParameter> PseudoParameter( IParameter pseudoParameter )
        {
            Invariant.Assert( pseudoParameter.GetRefFactory() == this );

            var accessor = (IMethod) pseudoParameter.DeclaringMember;

            Invariant.Assert( accessor.IsImplicitlyDeclared );

            if ( accessor.ContainingDeclaration is not IHasAccessors declaringMember )
            {
                throw new AssertionFailedException( $"Unexpected containing declaration: '{accessor.ContainingDeclaration}'." );
            }

            return new SymbolRef<IParameter>(
                declaringMember.GetSymbol().AssertSymbolNotNull(),
                this,
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

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from a Roslyn symbol.
        /// </summary>
        public SymbolRef<T> FromSymbol<T>(
            ISymbol symbol,
            RefTargetKind targetKind = RefTargetKind.Default )
            where T : class, ICompilationElement
            => (SymbolRef<T>)
                this._symbolCache.GetOrAdd(
                    new SymbolCacheKey( GetCanonicalSymbol( symbol ), targetKind ),
                    static ( key, me ) => new SymbolRef<T>( key.Symbol, me, key.TargetKind ),
                    this );

        public SymbolRef<IParameter> ReturnParameter( IMethodSymbol methodSymbol ) => this.FromSymbol<IParameter>( methodSymbol, RefTargetKind.Return );

        internal SymbolRef<ICompilation> ForCompilation() => this.FromSymbol<ICompilation>( this.CompilationContext.Compilation.Assembly );

        public SymbolRef<T> FromSymbolBasedDeclaration<T>( SymbolBasedDeclaration declaration )
            where T : class, IDeclaration
        {
            Invariant.Assert( declaration.GetRefFactory() == this );

            return this.FromSymbol<T>( declaration.Symbol );
        }
    }
}