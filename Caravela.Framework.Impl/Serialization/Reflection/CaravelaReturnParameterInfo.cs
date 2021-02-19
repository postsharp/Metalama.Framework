using Caravela.Framework.Impl.CodeModel;
using System.Reflection;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal class CaravelaReturnParameterInfo : ParameterInfo
    {
        public CaravelaReturnParameterInfo( Method.MethodReturnParameter returnParameter )
        {
            this.Method = returnParameter.DeclaringMethod;
        }

        public Method Method { get; }
    }
}