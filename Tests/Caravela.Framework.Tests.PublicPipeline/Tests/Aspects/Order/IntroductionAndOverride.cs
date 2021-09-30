using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Tests.Integration.TestInputs.Aspects.Order.IntroductionAndOverride;
using Caravela.TestFramework;

[assembly: AspectOrder(typeof(FirstAttribute), typeof(SecondAttribute), typeof(ThirdAttribute))]

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Order.IntroductionAndOverride
{
    public class FirstAttribute : Attribute, IAspect<INamedType>
    {
       
        
        
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var method in builder.Target.Methods)
            {
                builder.Advices.OverrideMethod(method, nameof(OverrideTemplate));
            }
        }

        [Introduce]
        public void IntroducedMethod1()
        {
            Console.Write("This is introduced by the first aspect.");
        }

        [Template]
        public dynamic? OverrideTemplate()
        {
            try
            {
                Console.Write("This is overridden by the first aspect.");
                return meta.Proceed();
            }
            finally
            {
                Console.Write("This is overridden by the first aspect.");
            }
        }
    }

    public class SecondAttribute : Attribute, IAspect<INamedType>
    {
       
        
        
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var method in builder.Target.Methods)
            {
                builder.Advices.OverrideMethod(method, nameof(OverrideTemplate));
            }
        }


        [Introduce]
        public void IntroducedMethod2()
        {
            Console.Write("This is introduced by the second aspect.");
        }

        [Template]
        public dynamic? OverrideTemplate()
        {
            try
            {
                Console.Write("This is overridden by the second aspect.");
                return meta.Proceed();
            }
            finally
            {
                Console.Write("This is overridden by the second aspect.");
            }
        }
    }

    public class ThirdAttribute : Attribute, IAspect<INamedType>
    {
       
        
        
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var method in builder.Target.Methods)
            {
                builder.Advices.OverrideMethod(method, nameof(OverrideTemplate));
            }
        }

        [Introduce]
        public void IntroducedMethod3()
        {
            Console.Write("This is introduced by the third aspect.");
        }

        [Template]
        public dynamic? OverrideTemplate()
        {
            try
            {
                Console.Write("This is overridden by the third aspect.");
                return meta.Proceed();
            }
            finally
            {
                Console.Write("This is overridden by the third aspect.");
            }
        }
    }

    // <target>
    [First]
    [Second]
    [Third]
    internal class TargetClass
    {
        public int Method()
        {
            return 42;
        }
    }
}
