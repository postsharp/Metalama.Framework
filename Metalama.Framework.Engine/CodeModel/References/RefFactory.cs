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
    internal class RefFactory
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
        public IRef<IDeclaration> FromBuilder( IDeclarationBuilder builder ) => new BuilderRef<IDeclaration>( builder, this._compilationContext );

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from a Roslyn symbol.
        /// </summary>
        public IRef<ICompilationElement> FromSymbol( ISymbol symbol ) => new SymbolRef<ICompilationElement>( symbol, this._compilationContext );

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
                    MethodKind.PropertySet when pseudoParameter.IsReturnParameter => DeclarationRefTargetKind.PropertySetReturnParameter,
                    MethodKind.PropertySet => DeclarationRefTargetKind.PropertySetParameter,
                    MethodKind.PropertyGet => DeclarationRefTargetKind.PropertyGetReturnParameter,
                    MethodKind.EventRaise when pseudoParameter.IsReturnParameter => DeclarationRefTargetKind.EventRaiseReturnParameter,
                    MethodKind.EventRaise => throw new NotImplementedException(
                        $"Getting the reference of a pseudo event raiser parameter is not implemented." ),
                    _ => throw new AssertionFailedException( $"Unexpected MethodKind: {accessor.MethodKind}." )
                } );
        }

        public IRef<T> FromSymbolId<T>( SymbolId symbolKey )
            where T : class, ICompilationElement
            => new StringRef<T>( symbolKey.Id, this._compilationContext );

        public IRef<T> FromDeclarationId<T>( SerializableDeclarationId id )
            where T : class, ICompilationElement
            => new StringRef<T>( id.Id, this._compilationContext );

        public IRef<T> FromTypeId<T>( SerializableTypeId id )
            where T : class, IType
            => new StringRef<T>( id.Id, this._compilationContext );

        /// <summary>
        /// Creates an <see cref="IRef{T}"/> from a Roslyn symbol.
        /// </summary>
        public IRef<T> FromSymbol<T>(
            ISymbol symbol,
            DeclarationRefTargetKind targetKind = DeclarationRefTargetKind.Default )
            where T : class, ICompilationElement
            => new SymbolRef<T>( symbol, this._compilationContext, targetKind );

        public IRef<IParameter> ReturnParameter( IMethodSymbol methodSymbol )
            => new SymbolRef<IParameter>( methodSymbol, this._compilationContext, DeclarationRefTargetKind.Return );

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