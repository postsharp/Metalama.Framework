using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Fields.MultipleDeclarators_OnePromoted
{
    public class TestAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.AdviceFactory.OverrideFieldOrProperty(builder.Target.Fields.OfName("B").Single(), nameof(PropertyTemplate));
        }

        [Template]
        public dynamic? PropertyTemplate 
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
    [Test]
    internal class TargetClass
    {
        public int A, B, C;
    }
}
