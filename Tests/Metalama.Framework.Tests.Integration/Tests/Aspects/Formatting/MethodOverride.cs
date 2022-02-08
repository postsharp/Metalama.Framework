using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly:AspectOrder(
    typeof(Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.Aspect1), 
    typeof(Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.Aspect2))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting
{
    public class Aspect1 : MethodAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Advices.OverrideMethod(builder.Target, nameof(Override));
        }

        [Template]
        public dynamic? Override()
        {
            Console.WriteLine(nameof(Aspect1));
            return meta.Proceed();
        }
    }

    public class Aspect2 : MethodAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Advices.OverrideMethod(builder.Target, nameof(Override));
        }

        [Template]
        public dynamic? Override()
        {
            Console.WriteLine(nameof(Aspect2));
            return meta.Proceed();
        }
    }

    // <target>
    public class Target
    {
        [Aspect1]
        [Aspect2]
        public void Foo()
        {
            // Intentionally indented.
                Console.WriteLine("Foo");
        }

        [Aspect1]
        [Aspect2]
        public int Bar()
        {
            // Intentionally indented.
                Console.WriteLine("Bar");
            return 42;
        }
    }
}
