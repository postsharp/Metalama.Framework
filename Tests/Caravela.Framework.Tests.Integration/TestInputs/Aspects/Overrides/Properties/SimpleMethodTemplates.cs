using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.SimpleMethodTemplates
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OverrideAttribute : Attribute, IAspect<IProperty>
    {
        void IAspect<IProperty>.BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.AdviceFactory.OverrideFieldOrPropertyAccessors(builder.TargetDeclaration, nameof(GetTemplate), nameof(SetTemplate));
        }

        [OverrideFieldOrPropertyGetTemplate]
        public dynamic GetTemplate()
        {
            Console.WriteLine("This is the overridden getter.");
            return meta.Proceed();
        }

        [OverrideFieldOrPropertySetTemplate]
        public void SetTemplate()
        {
            Console.WriteLine("This is the overridden setter.");
            var discard = meta.Proceed();
        }
    }

    [TestOutput]
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