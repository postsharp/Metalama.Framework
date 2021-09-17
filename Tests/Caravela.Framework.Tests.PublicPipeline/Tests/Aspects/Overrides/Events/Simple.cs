using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Events.Simple
{
    public class OverrideAttribute : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic value)
        {
            Console.WriteLine("This is the add template.");
            meta.Proceed();
        }

        public override void OverrideRemove(dynamic value)
        {
            Console.WriteLine("This is the remove template.");
            meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        private HashSet<EventHandler> handlers = new HashSet<EventHandler>();

        [Override]
        public event EventHandler Event
        {
            add
            {
                Console.WriteLine("This is the original add.");
                this.handlers.Add(value);
            }

            remove
            {
                Console.WriteLine("This is the original remove.");
                this.handlers.Remove(value);
            }
        }
    }
}