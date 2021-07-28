using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Aspects.Async.NormalTemplateOnAsyncMethod
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

        
    }

    class TargetCode
    {
        [Aspect]
        async Task<int> MethodReturningTaskOfInt(int a)
{
    global::System.Console.WriteLine("Before");
    var result = (await this.__MethodReturningTaskOfInt__OriginalImpl(a));
    global::System.Console.WriteLine("After");
    return (int)result;
}

private async Task<int> __MethodReturningTaskOfInt__OriginalImpl(int a)
        {
            return a;
        }
        
        [Aspect]
        async Task MethodReturningTaskd(int a)
{
    global::System.Console.WriteLine("Before");
    await this.__MethodReturningTaskd__OriginalImpl(a);
    object result = null;
    global::System.Console.WriteLine("After");
    return;
}

private async Task __MethodReturningTaskd__OriginalImpl(int a)
        {
            Console.WriteLine("Oops");
        }
        
        [Aspect]
        async ValueTask<int> MethodReturningValueTaskOfInt(int a)
{
    global::System.Console.WriteLine("Before");
    var result = (await this.__MethodReturningValueTaskOfInt__OriginalImpl(a));
    global::System.Console.WriteLine("After");
    return (int)result;
}

private async ValueTask<int> __MethodReturningValueTaskOfInt__OriginalImpl(int a)
        {
            return a;
        }
    }
}