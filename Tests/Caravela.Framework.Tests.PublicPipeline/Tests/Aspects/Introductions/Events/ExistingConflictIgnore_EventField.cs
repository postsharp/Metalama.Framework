using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Events.ExistingConflictIgnore_EventField
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Ignore)]
        public event EventHandler? ExistingEvent;

        [Introduce(WhenExists = OverrideStrategy.Ignore)]
        public static event EventHandler? ExistingEvent_Static;
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public event EventHandler? ExistingEvent
        {
            add
            {
                Console.WriteLine("This is original event.");
            }

            remove
            {
                Console.WriteLine("This is original event.");
            }
        }

        public static event EventHandler? ExistingEvent_Static
        {
            add
            {
                Console.WriteLine("This is original event.");
            }

            remove
            {
                Console.WriteLine("This is original event.");
            }
        }
    }
}
