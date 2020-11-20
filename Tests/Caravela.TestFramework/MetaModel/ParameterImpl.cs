using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.TestFramework.MetaModel
{
    internal class ParameterImpl : IParameter
    {
        private readonly IParameterSymbol _parameter;

        public ParameterImpl(IParameterSymbol parameter)
        {
            this._parameter = parameter;
        }

        public string Name => this._parameter.Name;

        public dynamic Value
        {
            get => new SimpleDynamicMetaMember(() => SyntaxFactory.IdentifierName(this._parameter.Name));
            set => throw new NotImplementedException();
        }

        public bool IsOut => this._parameter.RefKind == RefKind.Out;
    }
}