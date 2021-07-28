using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Aspects.Async.NormalTemplateOnVoidAsyncMethod
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }

    class TargetCode
    {
       [Aspect]
        async void MethodReturningValueTaskOfInt(int a)
{
    global::System.Console.WriteLine("Before");
    await this.__MethodReturningValueTaskOfInt__OriginalImpl(a);
    object result = null;
    global::System.Console.WriteLine("After");
    return;
}

private async global::System.Threading.Tasks.ValueTask __MethodReturningValueTaskOfInt__OriginalImpl(int a)
        {
            Console.WriteLine( "Oops" );
        }
    }
}