﻿#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.PartialMethod_SyntaxTrees
{
    /*
     * Tests that overriding partial methods in different syntax trees does work and targets correct methods.
     */
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var method in builder.Target.Methods)
            {
                builder.Advice.Override( method, nameof(Template), tags: new { name = method.Name } );
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

        partial void TargetMethodNoImplementation();

        partial void TargetVoidMethodWithImplementation();
    }
}