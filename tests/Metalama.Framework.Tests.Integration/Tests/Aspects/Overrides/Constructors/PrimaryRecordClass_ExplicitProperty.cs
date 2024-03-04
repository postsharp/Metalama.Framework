using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.PrimaryRecordClass_ExplicitProperty
{
    // Tests single OverrideConstructor advice on a primary constructor of a record class with a explicitly defined positional property.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var constructor in builder.Target.Constructors)
            {
                if (constructor.IsImplicitlyDeclared)
                {
                    continue;
                }

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
    public record class TargetClass(int X)
    {
        public int X = X;

        public void Foo()
        {
            this.X = 42;
        }
    }
}