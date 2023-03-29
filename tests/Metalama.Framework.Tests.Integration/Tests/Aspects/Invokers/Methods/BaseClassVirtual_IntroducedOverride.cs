#if TEST_OPTIONS
// @Include(_Shared.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassVirtual_IntroduceOverride;

/*
 * Tests that invoking a aspect-introduced method override is correctly referenced by invokers.
 */

public class BaseClass
{
    public virtual void Method(BaseClass instance, string value)
    {
    }
}

// <target>
[IntroductionAspect(nameof(Method), typeof(BaseClass), OverrideStrategy.Override )]
public class TargetClass : BaseClass
{
    [InvokerAspect(TargetName = nameof(Method), TargetLevel = 1)]
    public void InvokerMethod(BaseClass instance, string value)
    {
    }

    [InvokerAspectBeforeIntroduction(TargetName = nameof(Method), TargetLevel = 1)]
    public void InvokerMethodBeforeIntroduction(BaseClass instance, string value)
    {
    }
}
