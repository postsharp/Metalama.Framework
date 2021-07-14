﻿using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverrideBaseNonVirtual
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int BaseMethod()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static int BaseMethod_Static()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public int BaseMethod()
        {
            return 13;
        }

        public static int BaseMethod_Static()
        {
            return 13;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass
    {
    }
}
