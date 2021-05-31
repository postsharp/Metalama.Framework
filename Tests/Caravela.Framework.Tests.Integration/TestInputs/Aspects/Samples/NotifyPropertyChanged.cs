using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder)
        {
        }

        [Introduce]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Template]
        public dynamic SetPropertyTemplate()
        {
            var result = meta.Proceed();
            meta.This.PropertyChanged?.Invoke(meta.RunTime(meta.This), new PropertyChangedEventArgs( meta.RunTime(meta.Property.Name)));
            return result;
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
