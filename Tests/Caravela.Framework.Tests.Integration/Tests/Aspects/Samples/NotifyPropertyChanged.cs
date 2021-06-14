using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;
using System.ComponentModel;
using System.Linq;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Samples.NotifyPropertyChanged
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NotifyPropertyChangedAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.AdviceFactory.IntroduceInterface(builder.TargetDeclaration, typeof(INotifyPropertyChanged));

            foreach(var property in builder.TargetDeclaration.Properties
                .Where(p => p.Accessibility == Accessibility.Public && p.Writeability == Writeability.All))
            {
                builder.AdviceFactory.OverrideFieldOrPropertyAccessors(property, null, nameof(SetPropertyTemplate));
            }
        }

        [InterfaceMember]
        public event PropertyChangedEventHandler? PropertyChanged;
        
        [Introduce]
        protected virtual void OnPropertyChanged( string name )
        {
            meta.This.PropertyChanged?.Invoke(meta.This, new PropertyChangedEventArgs(meta.Parameters[0].Value));
        }

        [Template]
        public dynamic SetPropertyTemplate()
        {
            var value = meta.Parameters[0].Value;

            if (value != meta.Property.Value)
            {
                meta.This.OnPropertyChanged(meta.Property.Name);

                // TODO: Fix after Proceed refactoring (28573).
                var dummy = meta.Proceed();
            }

            return value;
        }
    }

    [TestOutput]
    [NotifyPropertyChanged]
    class Car
    {
        public string? Make { get; set; }
        public double Power { get; set; }

    }
}
