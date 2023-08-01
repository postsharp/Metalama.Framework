using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Async.AsyncTemplate.AsyncMethod_CrossAssembly
{
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