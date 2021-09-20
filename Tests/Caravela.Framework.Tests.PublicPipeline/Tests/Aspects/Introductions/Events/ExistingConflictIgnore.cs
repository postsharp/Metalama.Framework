﻿using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Events.ExistingConflictIgnore
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Ignore)]
        public event EventHandler ExistingEvent
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.Ignore)]
        public static event EventHandler ExistingEvent_Static
        {
            add
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced event.");
                meta.Proceed();
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
