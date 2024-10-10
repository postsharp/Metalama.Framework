// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Built;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// Builds instances of the <see cref="IRef{T}"/> interface.
    /// </summary>
    internal sealed class RefFactory
    {
        private readonly CompilationModel? _canonicalCompilationModel;

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
            => new BuiltDeclarationRef<T>( builder, this, genericContext );

        public FullRef<T> FromBuilt<T>( BuiltDeclaration builtDeclaration )
            where T : class, IDeclaration
            => this.FromBuilderData<T>( builtDeclaration.BuilderData, builtDeclaration.GenericContext );

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from a Roslyn symbol.
        /// </summary>
        public ISymbolRef<IDeclaration> FromDeclarationSymbol( ISymbol symbol ) => (ISymbolRef<IDeclaration>) this.FromAnySymbol( symbol );

        public ISymbolRef<ICompilationElement> FromAnySymbol( ISymbol symbol )
            => symbol.GetDeclarationKind( this.CompilationContext ) switch
            {
                DeclarationKind.Compilation => new SymbolRef<ICompilation>( symbol, this ),
                DeclarationKind.NamedType => new SymbolRef<INamedType>( symbol, this ),
                DeclarationKind.Method => new SymbolRef<IMethod>( symbol, this ),
                DeclarationKind.Property => new SymbolRef<IProperty>( symbol, this ),
                DeclarationKind.Indexer => new SymbolRef<IIndexer>( symbol, this ),
                DeclarationKind.Field => new SymbolRef<IField>( symbol, this ),
                DeclarationKind.Event => new SymbolRef<IEvent>( symbol, this ),
                DeclarationKind.Parameter => new SymbolRef<IParameter>( symbol, this ),
                DeclarationKind.TypeParameter => new SymbolRef<ITypeParameter>( symbol, this ),
                DeclarationKind.Attribute => new SymbolRef<IAttribute>( symbol, this ),
                DeclarationKind.ManagedResource => new SymbolRef<IManagedResource>( symbol, this ),
                DeclarationKind.Constructor => new SymbolRef<IConstructor>( symbol, this ),
                DeclarationKind.Finalizer => new SymbolRef<IMethod>( symbol, this ),
                DeclarationKind.Operator => new SymbolRef<IMethod>( symbol, this ),
                DeclarationKind.AssemblyReference => new SymbolRef<IAssembly>( symbol, this ),
                DeclarationKind.Namespace => new SymbolRef<INamespace>( symbol, this ),
                DeclarationKind.Type => new SymbolRef<IType>( symbol, this ),
                _ => throw new ArgumentOutOfRangeException()
            };

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
            => new( symbol, this, targetKind );

        public SymbolRef<IParameter> ReturnParameter( IMethodSymbol methodSymbol ) => new( methodSymbol, this, RefTargetKind.Return );

        internal SymbolRef<ICompilation> ForCompilation()
        {
            return this.FromSymbol<ICompilation>( this.CompilationContext.Compilation.Assembly );
        }

        public SymbolRef<T> FromSymbolBasedDeclaration<T>( SymbolBasedDeclaration declaration )
            where T : class, IDeclaration
        {
            Invariant.Assert( declaration.GetRefFactory() == this );

            return this.FromSymbol<T>( declaration.Symbol );
        }
    }
}