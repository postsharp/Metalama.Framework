using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug34547;

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender
            .SelectMany( compilation => compilation.AllTypes )
            .Where( type => type.Accessibility is Accessibility.Public )
            .SelectMany( type => type.Methods )
            .Where( method => method.Accessibility is Accessibility.Public )
            .AddAspectIfEligible<LogAttribute>();
    }
}

public class LogAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        return meta.Proceed();
    }
}

// <target>
public delegate void D();