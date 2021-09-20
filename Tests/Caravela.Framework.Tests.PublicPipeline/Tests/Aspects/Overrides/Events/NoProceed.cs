using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Events.NoProceed
{
    [AttributeUsage(AttributeTargets.Event, AllowMultiple = true)]
    public class OverrideAttribute : Attribute, IAspect<IEvent>
    {
        void IAspect<IEvent>.BuildAspect(IAspectBuilder<IEvent> builder)
        {
            builder.AdviceFactory.OverrideEventAccessors(builder.Target, nameof(AccessorTemplate), nameof(AccessorTemplate), null);
        }
        
        [Template]
        public void AccessorTemplate()
        {
            Console.WriteLine("This is the overridden accessor.");
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public event EventHandler Event
        {
            add
            {
                Console.WriteLine("This is the original accessor.");
            }

            remove
            {
                Console.WriteLine("This is the original accessor.");
            }
        }
    }
}