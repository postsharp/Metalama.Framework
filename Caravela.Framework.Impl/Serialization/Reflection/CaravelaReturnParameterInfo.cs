using System.Reflection;
using Caravela.Framework.Impl.CodeModel.Symbolic;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaReturnParameterInfo : ParameterInfo
    {
        public CaravelaReturnParameterInfo( Method.MethodReturnParameter returnParameter )
        {
            this.Method = returnParameter.Method;
        }

        public Method Method { get; }
    }
}