using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;
using System.Linq;
using System.ComponentModel;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.NotifyPropertyChanged
{
    class NotifyPropertyChangedAttribute : Attribute, IAspect<INamedType>
    {
#pragma warning disable CS0067
        
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.AdviceFactory.IntroduceInterface(builder.TargetDeclaration, typeof(INotifyPropertyChanged));

            foreach (var property in builder.TargetDeclaration.Properties
                .Where(p => p.Accessibility == Accessibility.Public && p.Writeability == Writeability.All))
            {
                builder.AdviceFactory.OverrideFieldOrPropertyAccessors( property, null, nameof(OverridePropertySetter) );
            }
        }

        [InterfaceMember]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Introduce( ConflictBehavior = ConflictBehavior.Ignore )]
        protected void OnPropertyChanged(string name)
        {
            meta.This.PropertyChanged?.Invoke(meta.This, new PropertyChangedEventArgs( meta.Parameters[0].Name ) );
        }

        [Template]
        dynamic OverridePropertySetter()
        {
            var value = meta.Parameters[0].Value;

            if ( value != meta.Property.Value )
            {
                meta.This.OnPropertyChanged(meta.Property.Name);
                
                // TODO: Fix after Proceed refactoring (28573).
                var dummy = meta.Proceed();
            }

            return value;
        }
        
#pragma warning restore CS0067        
    }

    [TestOutput]
    [NotifyPropertyChanged]
    class Car
    {
        public string? Make {get; set;}
        public double Power {get;set;}

    }
}
