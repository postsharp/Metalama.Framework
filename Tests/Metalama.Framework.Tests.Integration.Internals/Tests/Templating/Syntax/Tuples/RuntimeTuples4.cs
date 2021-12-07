using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Tuples.RunTimeTuples4
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var a = 1;
            var b = 2; 
            
            var namedItems = (a, b);
            Console.WriteLine(namedItems.a);
            
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}