using Caravela.Framework.Impl.CodeModel;
using System.Reflection;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeReturnParameterInfo : ParameterInfo
    {
        public CompileTimeReturnParameterInfo( Method.MethodReturnParameter returnParameter )
        {
            this.Method = returnParameter.DeclaringMethod;
        }

        public Method Method { get; }
    }
}