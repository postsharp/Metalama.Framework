using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS8603

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