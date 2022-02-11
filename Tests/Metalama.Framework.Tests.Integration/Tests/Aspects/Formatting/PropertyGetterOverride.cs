using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.PropertyGetterOverride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly:AspectOrder(typeof(Aspect1), typeof(Aspect2))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.PropertyGetterOverride
{
    public class Aspect1 : PropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.Advices.OverrideFieldOrPropertyAccessors(builder.Target, nameof(Override), null);
        }

        [Template]
        public dynamic? Override()
        {
            Console.WriteLine(nameof(Aspect1));
            return meta.Proceed();
        }
    }

    public class Aspect2 : PropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.Advices.OverrideFieldOrPropertyAccessors(builder.Target, nameof(Override), null);
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
        public int Foo
        {
            get
            {
                Console.WriteLine("Foo.get");
                return 42;
            }

            set
            {

                Console.WriteLine("Foo.set");
            }
        }

        [Aspect1]
        [Aspect2]
        public int Bar { get; set; }
    }
}
