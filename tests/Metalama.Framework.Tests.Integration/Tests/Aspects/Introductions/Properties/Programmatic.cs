﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.Programmatic
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advise.IntroduceProperty( builder.Target, nameof(AutoProperty), buildProperty: p => p.Accessibility = Accessibility.Public );
            builder.Advise.IntroduceProperty( builder.Target, nameof(Property), buildProperty: p => p.Accessibility = Accessibility.Public );

            builder.Advise.IntroduceProperty(
                builder.Target,
                "PropertyFromAccessors",
                nameof(GetPropertyTemplate),
                nameof(SetPropertyTemplate),
                buildProperty: p => p.Accessibility = Accessibility.Public );

            // TODO: Expression bodied template.
        }

        [Template]
        public int AutoProperty { get; set; }

        [Template]
        public int Property
        {
            get
            {
                Console.WriteLine( "Get" );

                return meta.Proceed();
            }

            set
            {
                Console.WriteLine( "Set" );
                meta.Proceed();
            }
        }

        [Template]
        public int GetPropertyTemplate()
        {
            Console.WriteLine( "Get" );

            return meta.Proceed();
        }

        [Template]
        public void SetPropertyTemplate( int value )
        {
            Console.WriteLine( "Set" );
            meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}