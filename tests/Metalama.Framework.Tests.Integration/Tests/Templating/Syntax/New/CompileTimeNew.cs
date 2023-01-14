using System;
using System.Text;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

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