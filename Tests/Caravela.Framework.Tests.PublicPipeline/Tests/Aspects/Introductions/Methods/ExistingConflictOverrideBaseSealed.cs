﻿using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverrideBaseSealed
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int ExistingMethod()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public virtual int ExistingMethod()
        {
            return default;
        }
    }

    internal class DerivedClass : BaseClass
    {
        public sealed override int ExistingMethod()
        {
            return default;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : DerivedClass
    {
    }
}
