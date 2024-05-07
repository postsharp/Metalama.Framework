using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Multiple;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(FirstOverrideAttribute), typeof(SecondOverrideAttribute), typeof(IntroduceAndOverrideAttribute) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Multiple
{
    /*
     * Tests that multiple aspects overriding the same field produce correct code.
     */

    public class FirstOverrideAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            builder.Advice.Override( builder.Target, nameof(Template) );
        }

        [Template]
        public dynamic? Template
        {
            get
            {
                Console.WriteLine( "First override." );

                return meta.Proceed();
            }

            set
            {
                Console.WriteLine( "First override." );
                meta.Proceed();
            }
        }
    }

    public class SecondOverrideAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            builder.Advice.Override( builder.Target, nameof(Template) );
        }

        [Template]
        public dynamic? Template
        {
            get
            {
                Console.WriteLine( "Second override." );

                return meta.Proceed();
            }

            set
            {
                Console.WriteLine( "Second override." );
                meta.Proceed();
            }
        }
    }

    public class IntroduceAndOverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Outbound.SelectMany( x => x.FieldsAndProperties ).AddAspect( x => new FirstOverrideAttribute() );
            builder.Outbound.SelectMany( x => x.FieldsAndProperties ).AddAspect( x => new SecondOverrideAttribute() );
        }

        [Introduce]
        public int IntroducedField;

        [Introduce]
        public readonly int IntroducedReadOnlyField;
    }

    // <target>
    [IntroduceAndOverride]
    internal class TargetClass
    {
        public int Field;

        public static int StaticField;

        public int InitializerField = 42;

        public readonly int ReadOnlyField;

        public TargetClass()
        {
            ReadOnlyField = 42;
        }
    }
}