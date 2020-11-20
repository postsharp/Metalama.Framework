using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.TestFramework.MetaModel
{
    internal class MethodAdviceContextImpl : IMethodAdviceContext
    {
        private readonly IMethodSymbol _method;
        private readonly ImmutableList<ParameterImpl> _parameters;

        public MethodAdviceContextImpl(IMethodSymbol method)
        {
            this._method = method;
            this._parameters = method.Parameters.Select(p => new ParameterImpl(p)).ToImmutableList();
        }

        public string Name => this._method.Name;

        public IReadOnlyList<IParameter> Parameters => this._parameters;

        public dynamic Invoke => new SimpleDynamicMetaMember(() => SyntaxFactory.IdentifierName(this._method.Name));
    }
}