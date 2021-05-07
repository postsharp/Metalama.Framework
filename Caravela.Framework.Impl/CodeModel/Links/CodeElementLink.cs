// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    /// <summary>
    /// Contains factory methods for the generic <see cref="CodeElementLink{T}"/>.
    /// </summary>
    internal static class CodeElementLink
    {
        /// <summary>
        /// Asserts that a given symbol is compatible with a given <see cref="ICodeElement"/> interface.
        /// </summary>
        /// <param name="symbol"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ISymbol AssertValidType<T>( this ISymbol symbol )
            where T : ICodeElement
        {
            Invariant.Implies(
                typeof( T ) == typeof( IConstructor ),
                symbol.GetCodeElementKind() == CodeElementKind.Constructor );

            Invariant.Implies(
                typeof( T ) == typeof( IMethod ),
                symbol.GetCodeElementKind() == CodeElementKind.Method );

            return symbol;
        }

        /// <summary>
        /// Creates a <see cref="CodeElementLink{T}"/> from a <see cref="CodeElementBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="TCodeElement"></typeparam>
        /// <typeparam name="TBuilder"></typeparam>
        /// <returns></returns>
        public static CodeElementLink<TCodeElement> FromBuilder<TCodeElement, TBuilder>( TBuilder builder )
            where TCodeElement : class, ICodeElement
            where TBuilder : CodeElementBuilder
        {
            return new( builder );
        }

        /// <summary>
        /// Creates a <see cref="CodeElementLink{T}"/> from a <see cref="CodeElementBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static CodeElementLink<ICodeElement> FromBuilder( CodeElementBuilder builder )
        {
            return new( builder );
        }

        /// <summary>
        /// Creates a <see cref="CodeElementLink{T}"/> from a Roslyn symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static CodeElementLink<ICodeElement> FromSymbol( ISymbol symbol )
        {
            return new( symbol );
        }

        /// <summary>
        /// Creates a <see cref="CodeElementLink{T}"/> from a Roslyn symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CodeElementLink<T> FromSymbol<T>( ISymbol symbol )
            where T : class, ICodeElement
        {
            return new( symbol );
        }

        public static CodeElementLink<ICodeElement> ReturnParameter( IMethodSymbol methodSymbol )
            => new( methodSymbol, CodeElementSpecialKind.ReturnParameter );

        internal static CodeElementLink<ICodeElement> Compilation() => new( null, CodeElementSpecialKind.Compilation );
    }

    /// <summary>
    /// The base implementation of <see cref="ICodeElementLink{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal readonly struct CodeElementLink<T> : ICodeElementLink<T>
        where T : class, ICodeElement
    {
        private readonly CodeElementSpecialKind _kind;

        internal CodeElementLink( ISymbol? symbol, CodeElementSpecialKind kind = CodeElementSpecialKind.Default )
        {
            this._kind = kind;

            if ( symbol != null )
            {
                symbol.AssertValidType<T>();
            }

            this.Target = symbol;
        }

        internal CodeElementLink( CodeElementBuilder builder )
        {
            this.Target = builder;
            this._kind = CodeElementSpecialKind.Default;
        }

        private CodeElementLink( object? target, CodeElementSpecialKind kind )
        {
            this.Target = target;
            this._kind = kind;
        }

        public object? Target { get; }

        public T GetForCompilation( CompilationModel compilation ) => GetForCompilation( this.Target, compilation, this._kind );

        internal static T GetForCompilation( object? link, CompilationModel compilation, CodeElementSpecialKind kind = CodeElementSpecialKind.Default )
            => link switch
            {
                null => kind == CodeElementSpecialKind.Compilation ? (T) (object) compilation : throw new AssertionFailedException(),
                ISymbol symbol => (T) compilation.Factory.GetCodeElement( symbol.AssertValidType<T>(), kind ),
                CodeElementBuilder builder => (T) compilation.Factory.GetCodeElement( builder ),

                _ => throw new AssertionFailedException()
            };

        public ISymbol? Symbol => this.Target as ISymbol;

        public override string ToString() => this.Target?.ToString() ?? "null";

        public CodeElementLink<TOut> Cast<TOut>()
            where TOut : class, ICodeElement
            => new( this.Target, this._kind );
    }
}