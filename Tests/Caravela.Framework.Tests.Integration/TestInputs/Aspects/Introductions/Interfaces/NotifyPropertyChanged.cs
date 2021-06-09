using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.NotifyPropertyChanged
{
    class NotifyPropertyChangedAttribute : Attribute, IAspect<INamedType>
    {
#pragma warning disable CS0067
        
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.AdviceFactory.IntroduceInterface(builder.TargetDeclaration, typeof(INotifyPropertyChanged));

            foreach ( var property in builder.TargetDeclaration.Properties.Where( p => p.Accessibility == Accessibility.Public ) )
            {
                builder.AdviceFactory.OverrideFieldOrPropertyAccessors( property, null, nameof(OverridePropertySetter) );
            }
        }

        [InterfaceMember]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Introduce( ConflictBehavior = ConflictBehavior.Ignore )]
        protected void OnPropertyChanged(string name)
        {
            // TODO: remove meta.RunTime (28716).
            meta.This.PropertyChanged?.Invoke(new PropertyChangedEventArgs(meta.RunTime( meta.Parameters[0].Name )));
        }

        [Template]
        dynamic OverridePropertySetter()
        {
            var value = meta.Parameters[0].Value;

            if ( value != meta.Property.GetValue( meta.This ) )
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
        string? _make;
        double _power;
        public string? Make { get => _make; set => _make = value; }
        public double Power { get => _power; set => _power = value; }

    }
}
