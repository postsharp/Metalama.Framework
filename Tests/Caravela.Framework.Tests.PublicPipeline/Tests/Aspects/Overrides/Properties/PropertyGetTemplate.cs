﻿using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyGetTemplate
{
    // Tests get-only property template.

    [AttributeUsage( AttributeTargets.Property )]
    public class OverrideAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            builder.Advices.OverrideFieldOrProperty( builder.Target, nameof(OverrideProperty) );
        }

        [Template]
        public dynamic? OverrideProperty
        {
            get
            {
                Console.WriteLine( $"This is the overridden setter." );

                return meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public int AutoProperty { get; set; }

        [Override]
        public static int Static_AutoProperty { get; set; }

        [Override]
        public int AutoProperty_Init { get; init; }

        [Override]
        public int AutoProperty_GetOnly { get; }

        [Override]
        public int Property
        {
            get
            {
                Console.WriteLine( "This is the original getter." );

                return 42;
            }

            set
            {
                Console.WriteLine( $"This is the original setter, setting {value}." );
            }
        }

        [Override]
        public static int Static_Property
        {
            get
            {
                Console.WriteLine( "This is the original getter." );

                return 42;
            }

            set
            {
                Console.WriteLine( $"This is the original setter, setting {value}." );
            }
        }

        [Override]
        public int InitProperty
        {
            get
            {
                Console.WriteLine( "This is the original getter." );

                return 42;
            }

            init
            {
                Console.WriteLine( $"This is the original setter, setting {value}." );
            }
        }

        [Override]
        public int Property_GetOnly
        {
            get
            {
                Console.WriteLine( "This is the original getter." );

                return 42;
            }
        }

        [Override]
        public int Property_SetOnly
        {
            set
            {
                Console.WriteLine( $"This is the original setter, setting {value}." );
            }
        }

        [Override]
        public int Property_InitOnly
        {
            init
            {
                Console.WriteLine( $"This is the original setter, setting {value}." );
            }
        }
    }
}