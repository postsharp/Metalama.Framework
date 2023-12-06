using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritanceCrossAssembly.Dependency;

[Inheritable]
internal class InheritableAspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( nameof(InheritableAspect1) );

        return meta.Proceed();
    }
}

[Inheritable]
internal class InheritableAspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(nameof(InheritableAspect2));

        return meta.Proceed();
    }
}

[Inheritable]
internal class InheritableAspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(nameof(InheritableAspect3));

        return meta.Proceed();
    }
}

[Inheritable]
internal class InheritableAspect4 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(nameof(InheritableAspect4));

        return meta.Proceed();
    }
}

public interface IInterfaceWithAspects
{
    [InheritableAspect1]
    [InheritableAspect2]
    [InheritableAspect3]
    [InheritableAspect4]
    void TargetMethod();
}