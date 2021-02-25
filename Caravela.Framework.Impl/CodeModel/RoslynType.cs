using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class RoslynType<T> : ITypeInternal
        where T : ITypeSymbol
    {
        protected CompilationModel Compilation { get; }

        public T Symbol { get; }

        protected RoslynType( T symbol, CompilationModel compilation )
        {
            this.Compilation = compilation;
            this.Symbol = symbol;
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this.Symbol.ToDisplayString();

        public abstract TypeKind TypeKind { get; }

        ICompilation IType.Compilation => this.Compilation;

        ITypeSymbol? ISdkType.TypeSymbol => this.Symbol;

        public bool Equals( IType other ) => this.Symbol.Equals( ((ITypeInternal) other).TypeSymbol );

        public override string ToString() => this.Symbol.ToString();
    }
}