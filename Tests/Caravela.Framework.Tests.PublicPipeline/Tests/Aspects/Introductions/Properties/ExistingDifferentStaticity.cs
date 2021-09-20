﻿using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingDifferentStaticity
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public static int ExistingProperty
        {
            get
            {
                Console.WriteLine("This is introduced property.");
                return 42;
            }
        }

        [Introduce]
        public int ExistingProperty_Static
        {
            get
            {
                Console.WriteLine("This is introduced property.");
                return 42;
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public int ExistingProperty
        {
            get
            {
                return 0;
            }
        }

        public static int ExistingProperty_Static
        {
            get
            {
                return 0;
            }
        }
    }
}
