using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;
using System.ComponentModel;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Samples.NotifyPropertyChanged
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NotifyPropertyChangedAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.AdviceFactory.IntroduceInterface(builder.TargetDeclaration, typeof(INotifyPropertyChanged));

            foreach(var property in builder.TargetDeclaration.Properties)
            {
                builder.AdviceFactory.OverrideFieldOrPropertyAccessors(property, null, nameof(SetPropertyTemplate));
            }
        }

        [InterfaceMember]
        public event PropertyChangedEventHandler? PropertyChanged;
        
        [Introduce]
        protected virtual void OnPropertyChanged( string name )
        {
            meta.This.PropertyChanged?.Invoke(meta.This, new PropertyChangedEventArgs(meta.Parameters[0].Name));
        }

        [Template]
        public void SetPropertyTemplate(dynamic value)
        {
            if ( value != meta.Property.Value )
            {
                meta.This.OnPropertyChanged( meta.Property.Name );
                var result = meta.Proceed();
            }
        }
    }

    [TestOutput]
    [NotifyPropertyChanged]
    internal class TargetClass
    {
        public int Property1 { get; set; }

        public int Property2 { get; set; }
    }
}
