using System.Linq;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.TestFramework.MetaModel
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