using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaParameterInfo : ParameterInfo
    {
        public ISymbol Symbol { get; }

        public CaravelaParameterInfo( ISymbol symbol )
        {
            this.Symbol = symbol;
        }
    }
}