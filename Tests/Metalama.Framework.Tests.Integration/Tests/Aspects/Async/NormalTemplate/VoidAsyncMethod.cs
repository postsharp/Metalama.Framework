// @IgnoredDiagnostic(CS1998)

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Aspects.Async.NormalTemplate.VoidAsyncMethod
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

    // <target>
    class TargetCode
    {
        [Aspect]
        async void MethodReturningVoid(int a)
        {
            await Task.Yield();
            Console.WriteLine( "Oops" );
        }
    }
}