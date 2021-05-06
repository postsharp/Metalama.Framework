using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.New.RunTimeNewClass
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var o = new TargetCode();
            Console.WriteLine(o.GetType().ToString());
            
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