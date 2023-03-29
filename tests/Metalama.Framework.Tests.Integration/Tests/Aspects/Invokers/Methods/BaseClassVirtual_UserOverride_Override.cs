#if TEST_OPTIONS
// @Include(_Shared.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassVirtual_UserOverride_Override;

public class BaseClass
{
    public virtual void Method(BaseClass instance, string value)
    {
    }
}

// <target>
[IntroductionAspect(nameof(Method), typeof(BaseClass), OverrideStrategy.Override)]
public class TargetClass : BaseClass
{
    [InvokerAspect(TargetName = nameof(Method), TargetLevel = 1)]
    public void InvokerMethod(BaseClass instance, string value)
    {
    }

    [OverrideBeforeInvokerAspect]
    [OverrideAfterInvokerAspect]
    public override void Method(BaseClass instance, string value)
    {
    }
}
