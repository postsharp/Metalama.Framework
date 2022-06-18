using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.CompileTimeParameters
{
    internal class MyAspect : TypeAspect
    {
        [Template]
        public T Template<[CompileTime] T>( IDeclaration d )
        {
            return default;
        }
    }
}