using Caravela.Framework.Code;
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

        ITypeFactory IType.TypeFactory => this.Compilation.Factory;

        ITypeSymbol ITypeInternal.TypeSymbol => this.Symbol;

        public override string ToString() => this.Symbol.ToString();
    }
}