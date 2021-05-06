﻿using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Tests.Integration.TestInputs.Aspects.Order.IntroductionAndOverride;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

[assembly: AspectOrder(typeof(FirstAttribute), typeof(SecondAttribute), typeof(ThirdAttribute))]

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Order.IntroductionAndOverride
{
    public class FirstAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
            foreach (var method in aspectBuilder.TargetDeclaration.Methods)
            {
                aspectBuilder.AdviceFactory.OverrideMethod(method, nameof(OverrideTemplate));
            }
        }

        [IntroduceMethod]
        public void IntroducedMethod1()
        {
            Console.Write("This is introduced by the first aspect.");
        }

        [OverrideMethodTemplate]
        public dynamic OverrideTemplate()
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
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
            foreach (var method in aspectBuilder.TargetDeclaration.Methods)
            {
                aspectBuilder.AdviceFactory.OverrideMethod(method, nameof(OverrideTemplate));
            }
        }

        [IntroduceMethod]
        public void IntroducedMethod2()
        {
            Console.Write("This is introduced by the second aspect.");
        }

        [OverrideMethodTemplate]
        public dynamic OverrideTemplate()
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
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
            foreach (var method in aspectBuilder.TargetDeclaration.Methods)
            {
                aspectBuilder.AdviceFactory.OverrideMethod(method, nameof(OverrideTemplate));
            }
        }

        [IntroduceMethod]
        public void IntroducedMethod3()
        {
            Console.Write("This is introduced by the third aspect.");
        }

        [OverrideMethodTemplate]
        public dynamic OverrideTemplate()
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

    [TestOutput]
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
