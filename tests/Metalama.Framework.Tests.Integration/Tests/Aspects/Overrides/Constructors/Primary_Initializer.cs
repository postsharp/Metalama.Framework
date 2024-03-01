﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Primary_Initializer
{
    // Tests single OverrideConstructor advice on primary constructors of a type with initializers using primary constructor parameters.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var constructor in builder.Target.Constructors)
            {
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
    public class TargetClass(int x, int y, EventHandler z)
    {
        private int a = x;

        private int B { get; } = y;

        private event EventHandler C = z;
    }
}