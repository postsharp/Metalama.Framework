using System.ComponentModel;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28809
{
    internal class IntroducePropertyChangedAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var newEvent = builder.Advise.IntroduceEvent(
                    builder.Target,
                    nameof(PropertyChanged) )
                .Declaration;

            builder.Advise.IntroduceMethod(
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
            @event.Invokers.Final.Raise( meta.This, meta.This, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    // <target>
    [IntroducePropertyChangedAspect]
    internal class TargetCode { }
}