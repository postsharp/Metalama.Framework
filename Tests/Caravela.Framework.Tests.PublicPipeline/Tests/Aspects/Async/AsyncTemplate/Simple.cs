using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Async.AsyncTemplate.Simple
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
        async Task<int> AsyncMethod(int a)
        {
            await Task.Yield();
            return a;
        }
    }
}