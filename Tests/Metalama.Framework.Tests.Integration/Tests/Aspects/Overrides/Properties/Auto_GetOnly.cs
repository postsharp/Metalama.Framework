using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;
using System;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly;


#pragma warning disable CS0169
#pragma warning disable CS0414

[assembly:AspectOrder(typeof(OverrideAttribute), typeof(IntroduceAndOverrideAttribute))]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly
{
    /*
     * Tests a single OverrideProperty aspect on get-only auto properties, including introduced get-only auto properties.
     */
    public class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        {
            get
            {
                Console.WriteLine("This is the overridden getter.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine($"This is the overridden setter.");
                meta.Proceed();
            }
        }
    }

    public class IntroduceAndOverrideAttribute : TypeAspect
    {     
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.With(x => x.Properties).AddAspect(x => new OverrideAttribute());
        }

        [Introduce]
        public int IntroducedProperty { get; }

        [Introduce]
        public int IntroducedStaticProperty { get; }
    }


    // <target>
    [IntroduceAndOverride]
    internal class TargetClass
    {
        public int Property { get; }

        public static int StaticProperty { get; }

        public int InitializerProperty { get; } = 42;

        public static int StaticInitializeProperty { get; } = 42;

        public TargetClass()
        {
            this.Property = 42;
            this.InitializerProperty = 27;
        }

        static TargetClass()
        {
            StaticProperty = 42;
            StaticInitializeProperty = 27;
        }
    }
}
