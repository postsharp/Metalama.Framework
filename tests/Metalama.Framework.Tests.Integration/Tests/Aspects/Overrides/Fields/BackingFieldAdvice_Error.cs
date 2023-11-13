using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.BackingFieldAdvice_Error
{
    public class TestAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
        {
            builder.Advice.Override(builder.Target, nameof(Template));
        }

        [Template]
        public dynamic? Template
        {
            get
            {
                Console.WriteLine("This is aspect code.");
                return meta.Proceed();
            }
            set
            {
                Console.WriteLine("This is aspect code.");
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [field: Test]
        public int AutoProperty { get; set; }
    }
}
