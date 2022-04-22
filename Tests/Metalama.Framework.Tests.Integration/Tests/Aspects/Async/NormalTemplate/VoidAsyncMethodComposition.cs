// @IgnoredDiagnostic(CS1998)

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Tests.Integration.Templating.Aspects.Async.NormalTemplate.VoidAsyncMethodComposition;

[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) ) ]

namespace Metalama.Framework.Tests.Integration.Templating.Aspects.Async.NormalTemplate.VoidAsyncMethodComposition
{
    class Aspect1 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Aspect1.Before");
            var result = meta.Proceed();
            Console.WriteLine("Aspect1.After");
            return result;
            
        }
    }
    
   class Aspect2 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Aspect2.Before");
            var result = meta.Proceed();
            Console.WriteLine("Aspect2.After");
            return result;
            
        }
    }


    // <target>
    class TargetCode
    {
       [Aspect1, Aspect2]
        async void MethodReturningValueTaskOfInt(int a)
        {
            await Task.Yield();
            Console.WriteLine( "Oops" );
        }
    }
}