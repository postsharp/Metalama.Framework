using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Async.AsyncTemplate.VoidAsyncMethod
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var result = meta.Proceed();
            Console.WriteLine($"result={result}");
            return result;
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
        [Aspect]
        void NonAsyncMethod()
        {
        }

        [Aspect]
        async void AsyncMethod()
        {
            await Task.Yield();
        }
    }
}
