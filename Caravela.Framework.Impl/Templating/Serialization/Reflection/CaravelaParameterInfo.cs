using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    [Obfuscation( Exclude = true )]
    internal class CaravelaParameterInfo : ParameterInfo
    {
        public IParameterSymbol Symbol { get; }
        public ICodeElement ContainingMember { get; }

        public CaravelaParameterInfo( IParameterSymbol symbol, ICodeElement containingMember )
        {
            this.Symbol = symbol;
            this.ContainingMember = containingMember;
        }
    }
}