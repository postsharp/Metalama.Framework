using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.SimpleMethodTemplates
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OverrideAttribute : Attribute, IAspect<IProperty>
    {
        void IAspect<IProperty>.Initialize(IAspectBuilder<IProperty> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.OverrideFieldOrPropertyAccessors(aspectBuilder.TargetDeclaration, nameof(GetTemplate), nameof(SetTemplate));
        }

        [OverrideFieldOrPropertyGetTemplate]
        public dynamic GetTemplate()
        {
            Console.WriteLine("This is the overridden getter.");
            return proceed();
        }

        [OverrideFieldOrPropertySetTemplate]
        public void SetTemplate()
        {
            Console.WriteLine($"This is the overridden setter.");
            var discard = proceed();
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