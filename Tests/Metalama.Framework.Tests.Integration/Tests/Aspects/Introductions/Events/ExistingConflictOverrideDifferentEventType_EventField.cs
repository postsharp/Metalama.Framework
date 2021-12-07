using System;
using Caravela.Framework.Aspects;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Events.ExistingConflictOverrideDifferentEventType_EventField
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override )]
        public event EventHandler? ExistingEvent;
    }

    internal class BaseClass
    {
        public virtual event UnhandledExceptionEventHandler? ExistingEvent
        {
            add
            {
                Console.WriteLine( "This is original event." );
            }

            remove
            {
                Console.WriteLine( "This is original event." );
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass { }
}