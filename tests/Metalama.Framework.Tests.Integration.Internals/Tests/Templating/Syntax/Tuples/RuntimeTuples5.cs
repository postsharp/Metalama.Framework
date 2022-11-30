using System;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Tuples.RunTimeTuples
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var items = (a : 1, b: 2, 3);
            Console.WriteLine(items.a);
           
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