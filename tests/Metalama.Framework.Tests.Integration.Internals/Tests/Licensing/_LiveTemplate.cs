using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.LiveTemplate
{
    [LiveTemplate]
    public class TestAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(TestAspect));
            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        private int TargetMethod(int a)
        {
            return a;
        }
    }
}