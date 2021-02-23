using System.Reflection;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeParameterInfo : ParameterInfo, IReflectionMockCodeElement
    {

        public IParameterSymbol ParameterSymbol { get; }

        public ICodeElement ContainingMember { get; }

        public CompileTimeParameterInfo( IParameterSymbol parameterSymbol, ICodeElement containingMember )
        {
            this.ParameterSymbol = parameterSymbol;
            this.ContainingMember = containingMember;
        }

        ISymbol IReflectionMockCodeElement.Symbol => this.ParameterSymbol;
    }
}