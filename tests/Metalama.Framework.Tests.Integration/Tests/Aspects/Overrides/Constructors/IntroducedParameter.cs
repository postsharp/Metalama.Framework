using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.IntroducedParameter
{
    // Tests single OverrideConstructor aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template), args: new { i = 1 });
            builder.Advice.IntroduceParameter(builder.Target.Constructors.Single(), "introduced", TypeFactory.GetType(SpecialType.Int32), TypedConstant.Create(42));
            builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template), args: new { i = 2 });
        }

        [Template]
        public void Template([CompileTime] int i)
        {
            Console.WriteLine( "This is the override {i}." );

            foreach (var param in meta.Target.Parameters)
            {
                Console.WriteLine( $"Param {param.Name} = {param.Value}" );
            }

            meta.Proceed();
        }
    }

    // <target>
    [Override]
    public class TargetClass
    {
        public TargetClass()
        {
            Console.WriteLine($"This is the original constructor.");
        }
    }
}