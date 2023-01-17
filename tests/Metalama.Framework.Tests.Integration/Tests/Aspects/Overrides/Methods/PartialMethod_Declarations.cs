﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.PartialMethod_Declarations
{
    /*
     * Tests that overriding partial methods does work and targets correct methods.
     */

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var method in builder.Target.Methods)
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
        public partial int TargetMethod();

        partial void TargetVoidMethodNoImplementation();

        partial void TargetVoidMethodWithImplementation();
    }

    // <target>
    internal partial class TargetClass
    {
        public partial int TargetMethod()
        {
            Console.WriteLine( "This is a partial method." );

            return 42;
        }

        partial void TargetVoidMethodWithImplementation()
        {
            Console.WriteLine( "This is a partial method." );
        }
    }
}