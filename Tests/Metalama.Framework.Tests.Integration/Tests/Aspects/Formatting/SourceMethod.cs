using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.SourceMethod
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var method in builder.Target.Methods)
            {
                builder.Advices.Override(method, nameof(this.OverrideMethod));
            }
        }

        [Template]
        private dynamic OverrideMethod()
        {
            if (meta.Target.Method.Invoke() > 0)
            {
                var z = meta.Proceed();
                return z - 1;
            }

            return 0;
        }
    }

    // <target>
    [TestAspect]
    public class Target
    {
        public int Foo()
        {
            return 10;
        }
    }
}
