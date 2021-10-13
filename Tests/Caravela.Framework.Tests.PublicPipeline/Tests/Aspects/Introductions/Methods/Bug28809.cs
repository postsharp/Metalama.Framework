using System.Collections.Generic;
using System.ComponentModel;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28809
{
    internal class IntroducePropertyChangedAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var eventBuilder = builder.Advices.IntroduceEvent(
                builder.Target,
                nameof(PropertyChanged) );

            builder.Advices.IntroduceMethod(
                builder.Target,
                nameof(OnPropertyChanged),
                tags: new Dictionary<string, object?> { { "event", eventBuilder } } );
        }

        [Template]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Template]
        protected virtual void OnPropertyChanged( string propertyName )
        {
            var @event = (IEvent)meta.Tags["event"]!;
            @event.Invokers.Final.Raise( meta.This, meta.This, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    // <target>
    [IntroducePropertyChangedAspect]
    internal class TargetCode { }
}