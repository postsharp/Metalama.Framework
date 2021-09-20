using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Events.ExistingDifferentStaticity
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public static event EventHandler ExistingEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
            }
        }

        [Introduce]
        public event EventHandler ExistingEvent_Static
        {
            add
            {
                Console.WriteLine("This is introduced event.");
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public event EventHandler ExistingEvent
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

        public static event EventHandler ExistingEvent_Static
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
