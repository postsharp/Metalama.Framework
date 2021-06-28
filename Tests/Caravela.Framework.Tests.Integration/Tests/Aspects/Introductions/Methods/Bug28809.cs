using System;
using System.ComponentModel;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28809
{
    class IntroducePropertyChangedAspect : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var eventBuilder = builder.AdviceFactory.IntroduceEvent(
                builder.TargetDeclaration, 
                nameof(PropertyChanged), 
                nameof(PropertyChanged));

            builder.AdviceFactory.IntroduceMethod(
                builder.TargetDeclaration,
                nameof(OnPropertyChanged),
                options: AdviceOptions.Default.AddTag("event", eventBuilder));
        }


        [Template]
        public event PropertyChangedEventHandler PropertyChanged;

        [Template]
        protected virtual void OnPropertyChanged( string propertyName )
        {
            var @event = (IEvent) meta.Tags["event"];
            @event.Invokers.Final.Raise(meta.This, meta.This, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    // <target>
    [IntroducePropertyChangedAspect]
    class TargetCode
    {
    }
}