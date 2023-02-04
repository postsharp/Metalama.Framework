using System.ComponentModel;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28809
{
    internal class IntroducePropertyChangedAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var newEvent = builder.Advice.IntroduceEvent(
                    builder.Target,
                    nameof(PropertyChanged) )
                .Declaration;

            builder.Advice.IntroduceMethod(
                builder.Target,
                nameof(OnPropertyChanged),
                tags: new { @event = newEvent } );
        }

        [Template]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Template]
        protected virtual void OnPropertyChanged( string propertyName )
        {
            var @event = (IEvent)meta.Tags["event"]!;
            @event.Raise( meta.This, meta.This, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    // <target>
    [IntroducePropertyChangedAspect]
    internal class TargetCode { }
}