using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Samples.NotifyPropertyChanged
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NotifyPropertyChangedAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.AdviceFactory.IntroduceInterface(builder.TargetDeclaration, builder.TargetDeclaration.Compilation.TypeFactory.GetTypeByReflectionName("System.ComponentModel.INotifyPropertyChanged"));

            foreach(var property in builder.TargetDeclaration.Properties)
            {
                builder.AdviceFactory.OverrideFieldOrPropertyAccessors(property, null, nameof(SetPropertyTemplate));
            }
        }

        [Introduce]
        public event PropertyChangedEventHandler? PropertyChanged;
        
        [Introduce]
        protected virtual void OnPropertyChanged( string name )
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs( name));
        }

        [Template]
        public void SetPropertyTemplate(dynamic value)
        {
            if ( value != meta.Property.Value )
            {
                this.OnPropertyChanged( meta.Property.Name );
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
