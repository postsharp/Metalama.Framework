using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Links
{
    internal readonly struct MemberLink<T> : IMemberLink<T> 
        where T : class, IMember
    {
        public MemberLink( ISymbol symbol )
        {
            CodeElementLink.AssertValidType<T>( symbol );
            
            this.LinkedObject = symbol;
        }
        
        public MemberLink( IMemberLink<T> link )
        {
            this.LinkedObject = link;
        }

        public object? LinkedObject { get; }

        public T GetForCompilation( CompilationModel compilation ) => CodeElementLink<T>.GetForCompilation( this.LinkedObject, compilation );


        public string Name =>
            this.LinkedObject switch
            {
                ISymbol symbol => symbol.Name,
                IMemberLink<T> link => link.Name,
                _ => throw new AssertionFailedException()
            };

        public override string ToString() => this.LinkedObject?.ToString() ?? "null";

    }
}