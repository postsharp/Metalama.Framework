using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Aspects.Async.TwoNormalTemplatesOnVoidAsyncMethod.cs
{
    class Aspect1 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
    
   class Aspect2 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }


    class TargetCode
    {
       [Aspect1, Aspect2]
        async void MethodReturningValueTaskOfInt(int a)
{
    global::System.Console.WriteLine("Aspect2.Before");
    await this.__Override__MethodReturningValueTaskOfInt__By__Aspect1(a);
    object result_1 = null;
    global::System.Console.WriteLine("Aspect2.After");
    return;
}

private async global::System.Threading.Tasks.ValueTask __MethodReturningValueTaskOfInt__OriginalImpl(int a)
        {
            await Task.Yield();
            Console.WriteLine( "Oops" );
        }


private async global::System.Threading.Tasks.ValueTask __Override__MethodReturningValueTaskOfInt__By__Aspect1(global::System.Int32 a)
{
    global::System.Console.WriteLine("Aspect1.Before");
    await this.__MethodReturningValueTaskOfInt__OriginalImpl(a);
    object result = null;
    global::System.Console.WriteLine("Aspect1.After");
    return;
}    }
}