﻿using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingDifferentStaticity
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder ) { }

       
        
        
        [Introduce]
        public static int ExistingMethod()
        {
            Console.WriteLine( "This is introduced method." );
            return 42;
        }

        [Introduce]
        public int ExistingMethod_Static()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
        public int ExistingMethod()
        {
            return 0;
        }

        public static int ExistingMethod_Static()
        {
            return 0;
        }
    }
}
