// @IgnoredDiagnostic(CS1998)

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Aspects.Async.TwoNormalTemplatesOnVoidAsyncMethod.cs
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