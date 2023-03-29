#if TEST_OPTIONS
// @Include(_Shared.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassVirtual;

/*
 * Tests that when invokers are targeting a virtual method that is declared in a base class:
 *   - DEFAULT means this.Method
 *   - BASE means base.Method
 *   - CURRENT means this.Method
 *   - FINAL means this.Method
 *   
 * On non-this instances:
 *   - DEFAULT means instance.Method
 *   - BASE is not usable
 *   - CURRENT is not usable
 *   - FINAL means instance.Method
 */

public class BaseClass
{
    public virtual void Method(BaseClass instance, string arg)
    {
        Console.WriteLine($"Original: {arg}");
    }
}

// <target>
public class TargetClass : BaseClass
{ 
    [InvokerAspect(TargetName = nameof(Method), TargetLevel = 1, InvokeParameterInstance = true)]
    public void InvokerMethod(TargetClass instance, string arg)
    {
    }
}
