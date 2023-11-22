using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Async.AsyncTemplate.AsyncMethod
{
    class Aspect : OverrideMethodAspect
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
        [Aspect]
        int NormalMethod(int a)
        {
            return a;
        }
        
        [Aspect]
        async Task<int> AsyncTaskResultMethod(int a)
        {
            await Task.Yield();
            return a;
        }

        [Aspect]
        async Task AsyncTaskMethod()
        {
            await Task.Yield();
        }
    }
}