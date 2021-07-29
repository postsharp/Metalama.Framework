// @IgnoredDiagnostic(CS1998)

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
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Before");
            var result = meta.Proceed();
            Console.WriteLine("After");
            return result;
            
        }
        
    }

    class TargetCode
    {
        [Aspect]
        async Task<int> MethodReturningTaskOfInt(int a)
        {
            await Task.Yield();
            return a;
        }
        
        [Aspect]
        async Task MethodReturningTaskd(int a)
        {
            await Task.Yield();
            Console.WriteLine("Oops");
        }
        
        [Aspect]
        async ValueTask<int> MethodReturningValueTaskOfInt(int a)
        {
            await Task.Yield();
            return a;
        }
    }
}