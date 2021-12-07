﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.MethodTemplates
{
    // Tests get-only property template.

    [AttributeUsage( AttributeTargets.Property )]
    public class OverrideAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            builder.Advices.OverrideFieldOrPropertyAccessors( builder.Target, nameof(GetProperty), nameof(SetProperty) );
        }

        [Template]
        public dynamic? GetProperty()
        {
            Console.WriteLine( $"This is the overridden getter." );

            return meta.Proceed();
        }

        [Template]
        public void SetProperty()
        {
            Console.WriteLine( $"This is the overridden setter." );
            meta.Proceed();
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