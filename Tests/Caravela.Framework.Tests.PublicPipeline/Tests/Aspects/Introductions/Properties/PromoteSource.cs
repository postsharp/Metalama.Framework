using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.PromoteSource
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PromoteAttribute : Attribute, IAspect<IFieldOrProperty>
    {
        public void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
        {
            builder.Advices.OverrideFieldOrProperty(builder.Target, nameof(Template));
        }

        [Template]
        public dynamic? Template
        {
            get
            {
                return meta.Proceed();
            }

            set
            {
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Promote]
        private int _field;

        public void Foo()
        {
            Console.WriteLine("Original code.");
        }
    }
}
