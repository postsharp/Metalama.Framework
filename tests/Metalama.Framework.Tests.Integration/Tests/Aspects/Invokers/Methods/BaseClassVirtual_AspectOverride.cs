﻿#if TEST_OPTIONS
// @Include(_Shared.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassVirtual_AspectOverride;

/*
 * Tests that when invokers are targeting a virtual method that is declared in a base class and C# override is declared in the current class:
 *   - DEFAULT means this.Method
 *   - BASE means source code of C# override
 *   - CURRENT means source code of C# override
 *   - FINAL means this.Method
 *   
 * On non-this instances:
 *   - DEFAULT means instance.Method
 *   - BASE is not usable
 *   - CURRENT is not usable
 *   - FINAL means instance.Method
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
[IntroductionAspect(nameof(Method), typeof(BaseClass), OverrideStrategy.Override)]
public class TargetClass : BaseClass
{
    // Targets the state after introduction.
    [InvokerAspect(TargetName = nameof(Method), TargetLevel = 0, InvokeParameterInstance = true)]
    public void InvokerMethod(BaseClass instance, string value)
    {
    }

    // Targets the state before introduction.
    [InvokerAspectBeforeIntroductionAttribute(TargetName = nameof(Method), TargetLevel = 1, InvokeParameterInstance = true)]
    public void InvokerMethodBeforeIntroduction(BaseClass instance, string value)
    {
    }
}
