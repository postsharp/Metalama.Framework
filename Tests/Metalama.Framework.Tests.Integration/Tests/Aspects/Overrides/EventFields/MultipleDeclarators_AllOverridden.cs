﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.MultipleDeclarators_AllOverridden
{
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.OverrideAccessors( builder.Target.Events.OfName( "A" ).Single(), nameof(OverrideAdd), nameof(OverrideRemove), null );
            builder.Advices.OverrideAccessors( builder.Target.Events.OfName( "B" ).Single(), nameof(OverrideAdd), nameof(OverrideRemove), null );
            builder.Advices.OverrideAccessors( builder.Target.Events.OfName( "C" ).Single(), nameof(OverrideAdd), nameof(OverrideRemove), null );
        }

        [Template]
        public void OverrideAdd( dynamic value )
        {
            Console.WriteLine( "This is the add template." );
            meta.Proceed();
        }

        [Template]
        public void OverrideRemove( dynamic value )
        {
            Console.WriteLine( "This is the remove template." );
            meta.Proceed();
        }
    }

    // <target>
    [Test]
    internal class TargetClass
    {
        public event EventHandler? A, B, C;
    }
}