using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel.Links
{


    internal static class CodeElementLink
    {

        public static ISymbol AssertValidType<T>( this ISymbol symbol)
            where T : ICodeElement
        {
            Invariant.Implies( 
                typeof(T) == typeof(IConstructor), 
                symbol is IMethodSymbol constructor && (constructor.MethodKind == MethodKind.Constructor || constructor.MethodKind == MethodKind.StaticConstructor ),
                "Expected a constructor symbol.");
                    
            Invariant.Implies( 
                typeof(T) == typeof(IMethod), 
                symbol is IMethodSymbol method && !(method.MethodKind == MethodKind.Constructor || method.MethodKind == MethodKind.StaticConstructor ),
                "Expected a constructor symbol.");

            return symbol;
        }


#pragma warning disable 618

        public static CodeElementLink<T> FromLink<T>( ICodeElementLink<T> link ) where T : class, ICodeElement
        {
            return new CodeElementLink<T>( link );
        }

        public static CodeElementLink<ICodeElement> FromLink( ICodeElementLink<ICodeElement> link )
        {
            return new CodeElementLink<ICodeElement>( link );
        }

        public static CodeElementLink<ICodeElement> FromSymbol( ISymbol symbol )
        {
            return new CodeElementLink<ICodeElement>( symbol );
        }

        public static CodeElementLink<T> FromSymbol<T>( ISymbol symbol ) where T : class, ICodeElement
        {
            return new CodeElementLink<T>( symbol );
        }

        public static CodeElementLink<T> FromCodeElement<T>( T codeElement ) where T : class, ICodeElement
        {
            return new CodeElementLink<T>( codeElement );
        }
#pragma warning restore 618
    }
    internal readonly struct CodeElementLink<T> : ICodeElementLink<T>
        where T : class, ICodeElement
    {
      

        [Obsolete("Use the factory method.")]
        internal CodeElementLink( ISymbol symbol )
        {
            CodeElementLink.AssertValidType<T>( symbol );      
            
            this.LinkedObject = symbol;
        }

        [Obsolete("Use the factory method.")]
        internal  CodeElementLink( ICodeElementLink<T> link )
        {
            this.LinkedObject = link;
        }
        
        [Obsolete("Use the factory method.")]
        internal  CodeElementLink( T link )
        {
            Invariant.Assert( link is ICodeElementLink<T>, "The type must implement ICodeElementLink" );
            this.LinkedObject = link;
        }

        
        public object? LinkedObject { get; }

        public T GetForCompilation( CompilationModel compilation ) =>
            GetForCompilation( this.LinkedObject, compilation ); 
        
        internal static T GetForCompilation( object? link, CompilationModel compilation )
            => link switch
            {
                ISymbol symbol => (T) compilation.Factory.GetCodeElement( symbol.AssertValidType<T>() ),
                
                ReturnParameterLink returnParameterLink => (T) compilation.Factory.GetMethod( returnParameterLink.Method ).ReturnParameter,

                // TODO: Get a compilation-consistent IMember from the IMemberBuilder.
                ICodeElementLink<T> codeElement => codeElement.GetForCompilation( compilation ),
                
                _ => throw new AssertionFailedException()
            };


        public ISymbol? Symbol => this.LinkedObject as ISymbol;

        public override string ToString() => this.LinkedObject?.ToString() ?? "null";
    }

}