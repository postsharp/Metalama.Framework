using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;
using System;
//using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly;

#pragma warning disable CS0169
#pragma warning disable CS0414

//[assembly: AspectOrder(typeof(OverrideAttribute), typeof(IntroduceAndOverrideAttribute))]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.ReadOnly
{
    /*
     * Tests that overriding of readonly fields is possible, including introduced readonly fields.
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

    // TODO: Introductions are currently broken.

    //public class IntroduceAndOverrideAttribute : TypeAspect
    //{
    //    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    //    {
    //        builder.With(x => x.Properties).AddAspect(x => new OverrideAttribute());
    //    }

    //    [Introduce]
    //    public readonly int IntroducedField;

    //    [Introduce]
    //    public readonly int IntroducedStaticField;
    //}

    // <target>
    //[IntroduceAndOverride]
    internal class TargetClass
    {
        // TODO: Remove override attribute after the introductions are fixed.

        [Override]
        public readonly int ReadOnlyField;

        [Override]
        public static readonly int StaticReadOnlyField;

        [Override]
        public readonly int InitializerReadOnlyField = 42;

        [Override]
        public static readonly int StaticInitializerReadOnlyField = 42;

        static TargetClass()
        {
            StaticReadOnlyField = 42;
            StaticInitializerReadOnlyField = 27;
        }

        public TargetClass()
        {
            this.ReadOnlyField = 42;
            this.InitializerReadOnlyField = 27;
        }
    }
}
