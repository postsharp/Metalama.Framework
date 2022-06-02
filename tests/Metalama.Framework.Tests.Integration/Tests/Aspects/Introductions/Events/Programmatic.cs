using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Events.Programmatic
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            {
                var propertyBuilder = builder.Advice.IntroduceEvent( builder.Target, nameof(EventField) );
                propertyBuilder.Accessibility = Accessibility.Public;
            }

            {
                var propertyBuilder = builder.Advice.IntroduceEvent( builder.Target, nameof(Event) );
                propertyBuilder.Accessibility = Accessibility.Public;
            }

            {
                var propertyBuilder = builder.Advice.IntroduceEvent(
                    builder.Target,
                    "EventFromAccessors",
                    nameof(AddEventTemplate),
                    nameof(RemoveEventTemplace) );

                propertyBuilder.Accessibility = Accessibility.Public;
            }

            // TODO: Expression bodied template.
        }

        [Template]
        public event EventHandler? EventField;

        [Template]
        public event EventHandler Event
        {
            add
            {
                Console.WriteLine( "Get" );
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine( "Set" );
                meta.Proceed();
            }
        }

        [Template]
        public void AddEventTemplate( EventHandler value )
        {
            Console.WriteLine( "Add" );
            meta.Proceed();
        }

        [Template]
        public void RemoveEventTemplace( EventHandler value )
        {
            Console.WriteLine( "Remove" );
            meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}