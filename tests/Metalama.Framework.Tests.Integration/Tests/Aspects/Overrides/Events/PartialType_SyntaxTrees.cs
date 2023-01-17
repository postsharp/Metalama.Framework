﻿#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Events.PartialType_SyntaxTrees
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var @event in builder.Target.Events)
            {
                builder.Advise.OverrideAccessors( @event, nameof(Template), nameof(Template), null, tags: new { name = @event.Name } );
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
        public event EventHandler TargetEvent1
        {
            add => Console.WriteLine( "This is TargetEvent1." );
            remove => Console.WriteLine( "This is TargetEvent1." );
        }
    }
}