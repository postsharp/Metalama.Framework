using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Attributes;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(OverrideAttribute), typeof(IntroductionAttribute) )]

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Attributes
{
    /*
     * Tests that overriding fields keeps all the existing attributes or moves them to the backing field where necessary.
     */

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var field in builder.Target.Fields)
            {
                builder.Advice.Override( field, nameof(Template) );
            }
        }

        [Template]
        public dynamic? Template
        {
            get
            {
                Console.WriteLine( "This is the overridden getter." );

                return meta.Proceed();
            }

            set
            {
                Console.WriteLine( "This is the overridden setter." );
                meta.Proceed();
            }
        }
    }

    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        [FieldOnly]
        [FieldAndProperty]
        public int IntroducedField;
    }

    [AttributeUsage( AttributeTargets.Field )]
    public class FieldOnlyAttribute : Attribute { }

    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    public class FieldAndPropertyAttribute : Attribute { }

    // <target>
    [Introduction]
    [Override]
    internal class TargetClass
    {
        [FieldOnly]
        [FieldAndProperty]
        public int Field;
    }
}