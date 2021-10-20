using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.SyntaxOrder;
using Caravela.TestFramework;

[assembly: AspectOrder(typeof(Override4Attribute), typeof(Override3Attribute), typeof(Override2Attribute), typeof(Introduction2Attribute), typeof(Override1Attribute), typeof(Introduction1Attribute))]

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.SyntaxOrder
{
    public class Introduction1Attribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public void Foo()
        {
            Console.WriteLine("This is introduced method.");
            meta.Proceed();
            meta.Proceed();
        }
    }

    public class Override1Attribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.OverrideMethod(builder.Target.Methods.OfName("Foo").Single(), nameof(Template));
        }

        [Template]
        public void Template()
        {
            Console.WriteLine("This is overridden (1) method.");
            meta.Proceed();
            meta.Proceed();
        }
    }

    public class Introduction2Attribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public void Bar()
        {
            Console.WriteLine("This is introduced method.");
            meta.Proceed();
            meta.Proceed();
        }
    }

    public class Override2Attribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.OverrideMethod(builder.Target.Methods.OfName("Foo").Single(), nameof(Template));
        }

        [Template]
        public void Template()
        {
            Console.WriteLine("This is overridden (2) method.");
            meta.Proceed();
            meta.Proceed();
        }
    }

    public class Override3Attribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.OverrideMethod(builder.Target.Methods.OfName("Bar").Single(), nameof(Template));
        }

        [Template]
        public void Template()
        {
            Console.WriteLine("This is overridden (3) method.");
            meta.Proceed();
            meta.Proceed();
        }
    }

    public class Override4Attribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.OverrideMethod(builder.Target.Methods.OfName("Foo").Single(), nameof(Template));
        }

        [Template]
        public void Template()
        {
            Console.WriteLine("This is overridden (4) method.");
            meta.Proceed();
            meta.Proceed();
        }
    }

    // <target>
    [Introduction1]
    [Introduction2]
    [Override1]
    [Override2]
    [Override3]
    [Override4]
    internal class TargetClass
    {
    }
}
