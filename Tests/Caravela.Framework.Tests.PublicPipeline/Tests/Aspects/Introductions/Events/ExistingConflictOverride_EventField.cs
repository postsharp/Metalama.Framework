using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Events.ExistingConflictOverride_EventField
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Override)]
        public event EventHandler? ExistingBaseEvent;

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public event EventHandler? ExistingEvent;

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static event EventHandler? ExistingEvent_Static;

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public event EventHandler? NotExistingEvent;

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static event EventHandler? NotExistingEvent_Static;
    }

    internal class BaseClass
    {
        public virtual event EventHandler? ExistingBaseEvent
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

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass
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
