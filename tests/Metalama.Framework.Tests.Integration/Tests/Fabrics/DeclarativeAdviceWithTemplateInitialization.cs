using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics.DeclarativeAdviceWithTemplateInitialization;

internal class BuildInfo
{
    private class Fabric : TypeFabric
    {
        [Introduce]
        public string? TargetFramework { get; } = meta.Target.Project.TargetFramework;

        [Introduce]
        public string? Configuration { get; } = meta.Target.Project.Configuration;
    }
}