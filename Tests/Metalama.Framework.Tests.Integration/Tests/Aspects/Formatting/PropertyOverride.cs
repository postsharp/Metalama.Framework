using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.PropertyOverride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly:AspectOrder(typeof(Aspect1), typeof(Aspect2))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.PropertyOverride
{
    public class Aspect1 : PropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.Advices.OverrideFieldOrProperty(builder.Target, nameof(Override));
        }

        [Template]
        public dynamic? Override
        {
            get
            {
                Console.WriteLine(nameof(Aspect1));
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine(nameof(Aspect1));
                meta.Proceed();
            }
        }
    }

    public class Aspect2 : PropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.Advices.OverrideFieldOrProperty(builder.Target, nameof(Override));
        }

        [Template]
        public dynamic? Override
        {
            get
            {
                Console.WriteLine(nameof(Aspect2));
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine(nameof(Aspect2));
                meta.Proceed();
            }
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
