using System;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples4
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var a = meta.CompileTime(1);
            var b = meta.CompileTime(2); 
            
            var namedItems = meta.CompileTime((a, b));
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