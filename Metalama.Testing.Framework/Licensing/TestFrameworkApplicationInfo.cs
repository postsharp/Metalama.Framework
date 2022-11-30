using Metalama.Backstage.Extensibility;

namespace Metalama.Testing.Framework.Licensing;

internal class TestFrameworkApplicationInfo : ApplicationInfoBase
{
    public TestFrameworkApplicationInfo() : base( typeof(TestFrameworkApplicationInfo).Assembly ) { }

    public override string Name => "Metalama.Testing.Framework";

    public override bool ShouldCreateLocalCrashReports => false;
}