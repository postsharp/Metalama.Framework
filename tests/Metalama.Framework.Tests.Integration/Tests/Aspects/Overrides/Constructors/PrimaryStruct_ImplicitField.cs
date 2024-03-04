using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.PrimaryStruct_ImplicitField
{
    // Tests single OverrideConstructor advice on a primary constructor of a non-record struct with a field defined by a parameter.

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

    // <target>
    [Override]
    public struct TargetStruct(int x, int y)
    {
        public int Foo() => x;

        public int Bar() => y;
    }
}