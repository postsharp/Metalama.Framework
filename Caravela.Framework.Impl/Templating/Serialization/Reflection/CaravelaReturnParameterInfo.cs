using Caravela.Framework.Impl.CodeModel;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaReturnParameterInfo : ParameterInfo
    {
        public CaravelaReturnParameterInfo( Method.MethodReturnParameter returnParameter ) => this.Method = returnParameter.Method;

        public Method Method { get; }
    }
}