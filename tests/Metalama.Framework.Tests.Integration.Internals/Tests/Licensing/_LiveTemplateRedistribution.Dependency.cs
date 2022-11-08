using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.LiveTemplateRedistribution.Dependency
{
    [LiveTemplate]
    public class TestAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by resdistributed " + nameof(TestAspect));
            return meta.Proceed();
        }
    }
}