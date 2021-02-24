using Caravela.Framework.Project;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    [CompileTime]
    public interface IAspectWeaver : IAspectDriver
    {
        CSharpCompilation Transform( AspectWeaverContext context );
    }
}
