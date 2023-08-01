using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Async.AsyncTemplate.AsyncMethod_CrossAssembly
{
    public class Aspect : OverrideMethodAspect
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
}