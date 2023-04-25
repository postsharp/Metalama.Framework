// This code tests that the warning is reported when aspect inheritance is and is not licensed
// and that the aspect is and is not inherited in such case, respectively.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritance;

[Inheritable]
class NonInstantiatedInheritableAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(nameof(NonInstantiatedInheritableAspect));
        return meta.Proceed();
    }
}

[Inheritable]
class InstantiatedInheritableAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(nameof(InstantiatedInheritableAspect));
        return meta.Proceed();
    }
}

[Inheritable]
class NonInstatiatedConditionallyInheritableAspect : OverrideMethodAspect, IConditionallyInheritableAspect
{
    public bool IsInheritable { get; init; }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(nameof(NonInstatiatedConditionallyInheritableAspect));
        return meta.Proceed();
    }
}

[Inheritable]
class InstatiatedConditionallyInheritableAspect : OverrideMethodAspect, IConditionallyInheritableAspect
{
    public bool IsInheritable { get; init; }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(nameof(InstatiatedConditionallyInheritableAspect));
        return meta.Proceed();
    }
}

class BaseClass
{
    [InstantiatedInheritableAspect]
    [InstatiatedConditionallyInheritableAspect(IsInheritable = true)]
    public virtual void TargetMethod()
    {
    }
}

class InheritingClass : BaseClass
{
    public override void TargetMethod()
    {
    }
}