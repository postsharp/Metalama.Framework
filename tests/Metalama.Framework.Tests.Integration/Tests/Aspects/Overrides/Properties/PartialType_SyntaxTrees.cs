﻿#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Properties.PartialType_SyntaxTrees
{
    /*
     * Tests that overriding properties of types with multiple partial declarations across multiple syntax trees does work and targets correct methods.
     */

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var property in builder.Target.Properties)
            {
                builder.Advise.OverrideAccessors( property, nameof(Template), nameof(Template), tags: new { name = property.Name } );
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
        public int TargetProperty1
        {
            get
            {
                Console.WriteLine( "This is TargetProperty1." );

                return 42;
            }

            set => Console.WriteLine( "This is TargetProperty1." );
        }
    }
}