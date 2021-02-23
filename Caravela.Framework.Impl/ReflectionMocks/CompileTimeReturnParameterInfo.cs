using System.Reflection;
using Caravela.Framework.Impl.CodeModel;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal class CompileTimeReturnParameterInfo : ParameterInfo
    {
        public CompileTimeReturnParameterInfo( MethodReturnParameter returnParameter )
        {
            this.Method = returnParameter.DeclaringMethod;
        }

        public Method Method { get; }
    }
}