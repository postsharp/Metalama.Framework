using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.AspectWorkbench
{
    internal class AdviceContextImpl : IAdviceContext
    {
        private IMethodSymbol _method;

        public AdviceContextImpl(IMethodSymbol method)
        {
            this._method = method;
            this.MethodAdviceContext = new MethodAdviceContextImpl(method);
            this.ProceedImpl = new ProceedImpl((MethodDeclarationSyntax) method.DeclaringSyntaxReferences.Single().GetSyntax());
        }

        public IMethodAdviceContext MethodAdviceContext
        {
            get;
        }

        public IProceedImpl ProceedImpl { get;  }
    }
}