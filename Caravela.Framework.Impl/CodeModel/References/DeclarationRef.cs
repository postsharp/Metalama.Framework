// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// Contains factory methods for the generic <see cref="DeclarationRef{T}"/>.
    /// </summary>
    internal static class DeclarationRef
    {
        /// <summary>
        /// Asserts that a given symbol is compatible with a given <see cref="IDeclaration"/> interface.
        /// </summary>
        /// <param name="symbol"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ISymbol AssertValidType<T>( this ISymbol symbol )
            where T : IDeclaration
        {
            Invariant.Implies(
                typeof(T) == typeof(IConstructor),
                symbol.GetDeclarationKind() == DeclarationKind.Constructor );

            Invariant.Implies(
                typeof(T) == typeof(IMethod),
                symbol.GetDeclarationKind() == DeclarationKind.Method );

            return symbol;
        }

        /// <summary>
        /// Creates a <see cref="DeclarationRef{T}"/> from a <see cref="DeclarationBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="TCodeElement"></typeparam>
        /// <typeparam name="TBuilder"></typeparam>
        /// <returns></returns>
        public static DeclarationRef<TCodeElement> FromBuilder<TCodeElement, TBuilder>( TBuilder builder )
            where TCodeElement : class, IDeclaration
            where TBuilder : IDeclarationBuilder
        {
            return new( builder );
        }

        /// <summary>
        /// Creates a <see cref="DeclarationRef{T}"/> from a <see cref="DeclarationBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static DeclarationRef<IDeclaration> FromBuilder( DeclarationBuilder builder )
        {
            return new( builder );
        }

        /// <summary>
        /// Creates a <see cref="DeclarationRef{T}"/> from a Roslyn symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static DeclarationRef<IDeclaration> FromSymbol( ISymbol symbol )
        {
            return new( symbol );
        }

        /// <summary>
        /// Creates a <see cref="DeclarationRef{T}"/> from a Roslyn symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static DeclarationRef<T> FromSymbol<T>( ISymbol symbol )
            where T : class, IDeclaration
        {
            return new( symbol );
        }

        public static DeclarationRef<IDeclaration> ReturnParameter( IMethodSymbol methodSymbol )
            => new( methodSymbol, DeclarationSpecialKind.ReturnParameter );

        internal static DeclarationRef<IDeclaration> Compilation() => new( null, DeclarationSpecialKind.Compilation );
    }

    /// <summary>
    /// The base implementation of <see cref="IDeclarationRef{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal readonly struct DeclarationRef<T> : IDeclarationRef<T>
        where T : class, IDeclaration
    {
        private readonly DeclarationSpecialKind _kind;

        internal DeclarationRef( ISymbol? symbol, DeclarationSpecialKind kind = DeclarationSpecialKind.Default )
        {
            this._kind = kind;

            if ( symbol != null )
            {
                symbol.AssertValidType<T>();
            }

            this.Target = symbol;
        }

        internal DeclarationRef( IDeclarationBuilder builder )
        {
            this.Target = builder;
            this._kind = DeclarationSpecialKind.Default;
        }

        private DeclarationRef( object? target, DeclarationSpecialKind kind )
        {
            this.Target = target;
            this._kind = kind;
        }

        public object? Target { get; }

        public T GetForCompilation( CompilationModel compilation ) => GetForCompilation( this.Target, compilation, this._kind );

        internal static T GetForCompilation( object? link, CompilationModel compilation, DeclarationSpecialKind kind = DeclarationSpecialKind.Default )
            => link switch
            {
                null => kind == DeclarationSpecialKind.Compilation ? (T) (object) compilation : throw new AssertionFailedException(),
                ISymbol symbol => (T) compilation.Factory.GetDeclaration( symbol.AssertValidType<T>(), kind ),
                DeclarationBuilder builder => (T) compilation.Factory.GetDeclaration( builder ),

                _ => throw new AssertionFailedException()
            };

        public ISymbol? Symbol => this.Target as ISymbol;

        public override string ToString() => this.Target?.ToString() ?? "null";

        public DeclarationRef<TOut> Cast<TOut>()
            where TOut : class, IDeclaration
            => new( this.Target, this._kind );
    }
}