﻿#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using System;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Target_Struct;
using Metalama.Framework.Code;

#pragma warning disable CS0067

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(OverrideAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Target_Struct
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var @event in builder.Target.Events)
            {
                builder.With( @event ).OverrideAccessors( nameof(OverrideAdd), nameof(OverrideRemove) );
            }
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
    internal struct TargetClass
    {
        public event EventHandler? Event;

        public static event EventHandler? StaticEvent;
    }
}