using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Primary_ImplicitField
{
    // Tests single OverrideConstructor advice with trivial template on primary constructor with field defined by parameter.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var constructor in builder.Target.Constructors)
            {
                builder.Advice.Override(constructor, nameof(Template));
            }
        }

        [Template]
        public void Template()
        {
            Console.WriteLine( "This is the override." );

            foreach (var param in meta.Target.Parameters)
            {
                Console.WriteLine($"Param {param.Name} = {param.Value}");
            }

            meta.Proceed();
        }
    }

    public class BaseClass
    {
        public BaseClass(int x) { }
    }

    // <target>
    [Override]
    public class TargetClass(int x, int y) : BaseClass(x)
    {
        public int Foo() => y;
    }
}