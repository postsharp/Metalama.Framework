using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Async.AsyncTemplate.VoidAsyncMethodComposition;

[assembly: AspectOrder(typeof(Aspect1), typeof(Aspect2))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Async.AsyncTemplate.VoidAsyncMethodComposition
{
    class Aspect1 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            return meta.Proceed();
        }

        public override async System.Threading.Tasks.Task<dynamic?> OverrideAsyncMethod()
        {
            await Task.Yield();
            var result = await meta.Proceed();
            Console.WriteLine($"result={result}");
            return result;

        }
    }

    class Aspect2 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            return meta.Proceed();
        }

        public override async System.Threading.Tasks.Task<dynamic?> OverrideAsyncMethod()
        {
            await Task.Yield();
            var result = await meta.Proceed();
            Console.WriteLine($"result={result}");
            return result;

        }
    }

    // <target>
    class TargetCode
    {
        [Aspect1]
        [Aspect2]
        async void AsyncMethod()
        {
            await Task.Yield();
        }
    }
}
