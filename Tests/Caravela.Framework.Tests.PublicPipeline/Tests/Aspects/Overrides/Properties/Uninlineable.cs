using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;
using Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable;

[assembly: AspectOrder(typeof(FirstOverrideAttribute), typeof(SecondOverrideAttribute))]

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FirstOverrideAttribute : Attribute, IAspect<IProperty>
    {
        void IAspect<IProperty>.BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.Advices.OverrideFieldOrPropertyAccessors(builder.Target, nameof(GetTemplate), nameof(SetTemplate));
        }
        
        [Template]
        public dynamic? GetTemplate()
        {
            Console.WriteLine("This is the overridden getter.");
            _ = meta.Proceed();
            return meta.Proceed();
        }

        [Template]
        public void SetTemplate()
        {
            Console.WriteLine("This is the overridden setter.");
            meta.Proceed();
            meta.Proceed();
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SecondOverrideAttribute : Attribute, IAspect<IProperty>
    {
        void IAspect<IProperty>.BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.Advices.OverrideFieldOrPropertyAccessors(builder.Target, nameof(GetTemplate), nameof(SetTemplate));
        }

        [Template]
        public dynamic? GetTemplate()
        {
            Console.WriteLine("This is the overridden getter.");
            _ = meta.Proceed();
            return meta.Proceed();
        }

        [Template]
        public void SetTemplate()
        {
            Console.WriteLine("This is the overridden setter.");
            meta.Proceed();
            meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        private int _field;

        [FirstOverride]
        [SecondOverride]
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