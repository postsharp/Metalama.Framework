using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    /// <summary>
    /// The implementation of <see cref="IMemberLink{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal readonly struct MemberLink<T> : IMemberLink<T> 
        where T : class, IMember
    {
        public MemberLink( ISymbol symbol )
        {
            CodeElementLink.AssertValidType<T>( symbol );
            
            this.Target = symbol;
        }
        
        public MemberLink( IMemberLink<T> link )
        {
            this.Target = link;
        }

        public object? Target { get; }

        public T GetForCompilation( CompilationModel compilation ) => CodeElementLink<T>.GetForCompilation( this.Target, compilation );


        public string Name =>
            this.Target switch
            {
                ISymbol symbol => symbol.Name,
                IMemberLink<T> link => link.Name,
                _ => throw new AssertionFailedException()
            };

        public override string ToString() => this.Target?.ToString() ?? "null";

    }
}