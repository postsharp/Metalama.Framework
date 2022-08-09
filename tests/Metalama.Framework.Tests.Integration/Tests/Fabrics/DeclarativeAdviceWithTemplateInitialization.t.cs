using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics.DeclarativeAdviceWithTemplateInitialization;
#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414

internal class BuildInfo
{
#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
    private class Fabric : TypeFabric
    {
        [Introduce]
        public string? TargetFramework { get; }
        [Introduce]
        public string? Configuration { get; }
    }

#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414


    public global::System.String? Configuration { get; } = "Debug";

public global::System.String? TargetFramework { get; } = "net6.0";}
#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
