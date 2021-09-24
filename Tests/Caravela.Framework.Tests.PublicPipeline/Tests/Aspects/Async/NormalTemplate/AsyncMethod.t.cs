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
    var result = (await this.MethodReturningTaskOfInt_Source(a));
    global::System.Console.WriteLine("After");
    return (global::System.Int32)result;
}

private async Task<int> MethodReturningTaskOfInt_Source(int a)
        {
            await Task.Yield();
            return a;
        }
        
        [Aspect]
        async Task MethodReturningTaskd(int a)
{
    global::System.Console.WriteLine("Before");
    await this.MethodReturningTaskd_Source(a);
    object result = null;
    global::System.Console.WriteLine("After");
    return;
}

private async Task MethodReturningTaskd_Source(int a)
        {
            await Task.Yield();
            Console.WriteLine("Oops");
        }
        
        [Aspect]
        async ValueTask<int> MethodReturningValueTaskOfInt(int a)
{
    global::System.Console.WriteLine("Before");
    var result = (await this.MethodReturningValueTaskOfInt_Source(a));
    global::System.Console.WriteLine("After");
    return (global::System.Int32)result;
}

private async ValueTask<int> MethodReturningValueTaskOfInt_Source(int a)
        {
            await Task.Yield();
            return a;
        }
    }
}