﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Multiple;

[assembly: AspectOrder( typeof(FirstOverrideAttribute), typeof(SecondOverrideAttribute), typeof(IntroduceAndOverrideAttribute) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Multiple
{
    /*
     * Tests that multiple aspects overriding the same field produce correct code.
     */

    public class FirstOverrideAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
        {
            builder.Advice.Override(builder.Target, nameof(Template));
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
                Console.WriteLine("First override.");
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
                Console.WriteLine("Second override.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("Second override.");
                meta.Proceed();
            }
        }
    }

    public class IntroduceAndOverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.With(x => x.Fields).AddAspect(x => new FirstOverrideAttribute());
            builder.With(x => x.Fields).AddAspect(x => new SecondOverrideAttribute());
        }

        [Introduce]
        public int IntroducedField;

        [Introduce]
        public readonly int IntroducedReadOnlyField;
    }

    // <target>
    //[IntroduceAndOverride]
    internal class TargetClass
    {
        //[FirstOverride]
        //[SecondOverride]
        //public int Field;

        //[FirstOverride]
        //[SecondOverride]
        //public int StaticField;

        //[FirstOverride]
        //[SecondOverride]
        //public int InitializerField = 42;

        [FirstOverride]
        [SecondOverride]
        public readonly int ReadOnlyField;

        public TargetClass()
        {
            this.ReadOnlyField = 42;
        }
    }
}