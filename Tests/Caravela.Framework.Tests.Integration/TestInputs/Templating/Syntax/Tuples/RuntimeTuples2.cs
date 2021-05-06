using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.RunTimeTuples2
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var t = (1, 2, 3);
            Console.WriteLine(t.Item3);
            
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