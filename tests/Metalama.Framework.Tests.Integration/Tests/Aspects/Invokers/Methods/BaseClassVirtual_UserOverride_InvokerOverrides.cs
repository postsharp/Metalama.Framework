#if TEST_OPTIONS
// @Include(_Shared.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassVirtual_UserOverride_InvokerOverrides;

/*
 * Tests that when invokers are targeting a virtual method that is declared in a base class and C# override is declared in the current class:
 *   - DEFAULT means this.Method
 *   - BASE means source code of aspect override
 *   - CURRENT means the state after the second aspect override
 *   - FINAL means this.Method
 */

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
    [InvokerAspect(TargetName = nameof(Method), TargetLevel = 1, OverrideTargetBefore = true, OverrideTargetAfter = true )]
    public void InvokerMethod(BaseClass instance, string value)
    {
    }

    public override void Method(BaseClass instance, string value)
    {
    }
}
