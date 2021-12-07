using System;
using System.Text;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.New.CompileTimeNew
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var o = meta.CompileTime(new StringBuilder());
            o.Append("x");
            Console.WriteLine(o.ToString());
            
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