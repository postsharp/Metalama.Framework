using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Microsoft.CodeAnalysis;
using System;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

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
        public static ISymbol AssertValidType<T>( this ISymbol symbol)
            where T : ICodeElement
        {
            Invariant.Implies( 
                typeof(T) == typeof(IConstructor), 
                symbol.GetCodeElementKind() == CodeElementKind.Constructor);
                    
            Invariant.Implies( 
                typeof(T) == typeof(IMethod), 
                symbol.GetCodeElementKind() == CodeElementKind.Method);

            return symbol;
        }


#pragma warning disable 618

        /// <summary>
        /// Creates a <see cref="CodeElementLink{T}"/> from a <see cref="ICodeElementLink{T}"/> (typically a <see cref="CodeElementBuilder"/>).
        /// </summary>
        /// <param name="link"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CodeElementLink<T> FromLink<T>( ICodeElementLink<T> link ) where T : class, ICodeElement
        {
            return new CodeElementLink<T>( link );
        }

        /// <summary>
        /// Creates a <see cref="CodeElementLink{T}"/> from a <see cref="ICodeElementLink{T}"/> (typically a <see cref="CodeElementBuilder"/>).
        /// </summary>
        /// <param name="link"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CodeElementLink<ICodeElement> FromLink( ICodeElementLink<ICodeElement> link )
        {
            return new CodeElementLink<ICodeElement>( link );
        }

        /// <summary>
        /// Creates a <see cref="CodeElementLink{T}"/> from a Roslyn symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CodeElementLink<ICodeElement> FromSymbol( ISymbol symbol )
        {
            return new CodeElementLink<ICodeElement>( symbol );
        }

        /// <summary>
        /// Creates a <see cref="CodeElementLink{T}"/> from a Roslyn symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static CodeElementLink<T> FromSymbol<T>( ISymbol symbol ) where T : class, ICodeElement
        {
            return new CodeElementLink<T>( symbol );
        }

#pragma warning restore 618
    }
    
    /// <summary>
    /// The base implementation of <see cref="ICodeElementLink{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal readonly struct CodeElementLink<T> : ICodeElementLink<T>
        where T : class, ICodeElement
    {
      

        [Obsolete("Use the factory method.")]
        internal CodeElementLink( ISymbol symbol )
        {
            CodeElementLink.AssertValidType<T>( symbol );      
            
            this.Target = symbol;
        }

        [Obsolete("Use the factory method.")]
        internal CodeElementLink( ICodeElementLink<T> link )
        {
            this.Target = link;
        }
        
        [Obsolete("Use the factory method.")]
        internal CodeElementLink( T link )
        {
            Invariant.Assert( link is ICodeElementLink<T> );
            this.Target = link;
        }

        public object? Target { get; }

        public T GetForCompilation( CompilationModel compilation ) =>
            GetForCompilation( this.Target, compilation ); 
        
        internal static T GetForCompilation( object? link, CompilationModel compilation )
            => link switch
            {
                ISymbol symbol => (T) compilation.Factory.GetCodeElement( symbol.AssertValidType<T>() ),
                
                ReturnParameterLink returnParameterLink => (T) compilation.Factory.GetMethod( returnParameterLink.Method ).ReturnParameter,

                ICodeElementLink<T> codeElement => codeElement.GetForCompilation( compilation ),
                
                _ => throw new AssertionFailedException()
            };


        public ISymbol? Symbol => this.Target as ISymbol;

        public override string ToString() => this.Target?.ToString() ?? "null";
    }

}