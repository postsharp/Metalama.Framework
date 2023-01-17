﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.PartialType_Declarations
{
    /*
     * Tests that overriding operators of types with multiple partial declarations does work and targets correct operators.
     */

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var method in builder.Target.Methods.OfKind( MethodKind.Operator ))
            {
                builder.Advise.Override( method, nameof(Template), tags: new { name = method.Name } );
            }
        }

        [Template]
        public dynamic? Template()
        {
            Console.WriteLine( $"This is the override of {meta.Tags["name"]}." );

            return meta.Proceed();
        }
    }

    // <target>
    [Override]
    internal partial class TargetClass
    {
        public static TargetClass operator +(TargetClass a, TargetClass b)
        {
            Console.WriteLine($"This is the original operator.");

            return new TargetClass();
        }
    }

    // <target>
    internal partial class TargetClass
    {
        public static TargetClass operator -(TargetClass a, TargetClass b)
        {
            Console.WriteLine($"This is the original operator.");

            return new TargetClass();
        }
    }

    // <target>
    internal partial class TargetClass
    {
        public static TargetClass operator *(TargetClass a, TargetClass b)
        {
            Console.WriteLine($"This is the original operator.");

            return new TargetClass();
        }
    }
}