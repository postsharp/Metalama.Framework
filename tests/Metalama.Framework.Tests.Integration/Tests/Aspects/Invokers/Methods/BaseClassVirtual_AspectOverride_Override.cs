#if TEST_OPTIONS
// @Include(_Shared.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassVirtual_AspectOverride_Override;

/*
 * Tests that when invokers are targeting a virtual method that is declared in a base class and C# override is declared in the current class:
 *   - DEFAULT means this.Method
 *   - BASE means source code of aspect override
 *   - CURRENT means source code of aspect override
 *   - FINAL means this.Method
 *   
 * Also if the override target the base method or it's override the result should be identical.
 */

public class BaseClass
{
    public virtual void Method(BaseClass instance, string value)
    {
    }
}

// <target>
[IntroductionAspect(nameof(Method), typeof(BaseClass), OverrideStrategy.Override, OverrideTargetAfter = true)]
public class TargetClass : BaseClass
{
    [InvokerAspect(TargetName = nameof(Method), TargetLevel = 0)]
    public void InvokerMethod(BaseClass instance, string value)
    {
    }
}
