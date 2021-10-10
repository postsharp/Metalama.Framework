using System;
using Caravela.Framework.Aspects;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Events.ExistingDifferentStaticity_EventField
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public static event EventHandler? ExistingEvent;

        [Introduce]
        public event EventHandler? ExistingEvent_Static;
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public event EventHandler? ExistingEvent
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

        public static event EventHandler? ExistingEvent_Static
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
}