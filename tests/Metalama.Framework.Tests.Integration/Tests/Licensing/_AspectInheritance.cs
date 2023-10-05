using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritance;

[Inheritable]
internal class NonInstantiatedInheritableAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( nameof(NonInstantiatedInheritableAspect) );

        return meta.Proceed();
    }
}

[Inheritable]
internal class InstantiatedInheritableAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( nameof(InstantiatedInheritableAspect) );

        return meta.Proceed();
    }
}

[Inheritable]
internal class NonInstatiatedConditionallyInheritableAspect : OverrideMethodAspect, IConditionallyInheritableAspect
{
    public bool IsInheritable { get; init; }

    bool IConditionallyInheritableAspect.IsInheritable( IDeclaration targetDeclaration, IAspectInstance aspectInstance ) => IsInheritable;

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( nameof(NonInstatiatedConditionallyInheritableAspect) );

        return meta.Proceed();
    }
}

[Inheritable]
internal class InstatiatedConditionallyInheritableAspect : OverrideMethodAspect, IConditionallyInheritableAspect
{
    public bool IsInheritable { get; init; }

    bool IConditionallyInheritableAspect.IsInheritable( IDeclaration targetDeclaration, IAspectInstance aspectInstance ) => IsInheritable;

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( nameof(InstatiatedConditionallyInheritableAspect) );

        return meta.Proceed();
    }
}

internal class BaseClass
{
    [InstantiatedInheritableAspect]
    [InstatiatedConditionallyInheritableAspect( IsInheritable = true )]
    public virtual void TargetMethod() { }
}

internal class InheritingClass : BaseClass
{
    public override void TargetMethod() { }
}