﻿using Metalama.Framework.Aspects;
using System;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Unused;
using Metalama.Framework.Code;

#pragma warning disable CS0067

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(OverrideAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Unused
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var @event in builder.Target.Events)
            {
                builder.Advice.OverrideAccessors( @event, nameof(OverrideAdd), nameof(OverrideRemove) );
            }
        }

        [Template]
        public void OverrideAdd( dynamic value )
        {
            Console.WriteLine( "This is the add template." );
        }

        [Template]
        public void OverrideRemove( dynamic value )
        {
            Console.WriteLine( "This is the remove template." );
        }
    }

    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public event EventHandler? IntroducedEvent;

        [Introduce]
        public static event EventHandler? IntroducedStaticEvent;
    }

    // <target>
    [Override]
    [Introduction]
    internal class TargetClass
    {
        public event EventHandler? Event;

        public static event EventHandler? StaticEvent;
    }
}