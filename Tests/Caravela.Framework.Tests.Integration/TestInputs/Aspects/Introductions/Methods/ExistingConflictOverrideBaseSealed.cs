﻿using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverrideBaseSealed
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.Override)]
        public int ExistingMethod()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }
    }

    internal class BaseClass
    {
        public virtual void ExistingMethod()
        {
        }
    }

    internal class DerivedClass : BaseClass
    {
        public sealed override void ExistingMethod()
        {
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass : DerivedClass
    {
    }
}
