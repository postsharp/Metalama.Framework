using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.NoProceed
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class OverrideAttribute : Attribute, IAspect<IProperty>
    {
        void IAspect<IProperty>.BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.Advices.OverrideFieldOrPropertyAccessors(builder.Target, nameof(GetTemplate), nameof(SetTemplate));
        }
        
        [Template]
        public dynamic? GetTemplate()
        {
            Console.WriteLine("This is the overridden getter.");
            return default;
        }

        [Template]
        public void SetTemplate()
        {
            Console.WriteLine("This is the overridden setter.");
        }
    }

    // <target>
    internal class TargetClass
    {
        private int _field;

        [Override]
        public int Property
        {
            get
            {
                return this._field;
            }

            set
            {
                this._field = value;
            }
        }
    }
}