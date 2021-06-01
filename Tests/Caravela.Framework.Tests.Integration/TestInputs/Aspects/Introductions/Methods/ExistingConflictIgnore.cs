﻿using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictIgnore
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
       
        
        public void BuildAspectClass( IAspectClassBuilder builder ) { }

       

        [Introduce(ConflictBehavior = ConflictBehavior.Ignore)]
        public int ExistingMethod()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(ConflictBehavior = ConflictBehavior.Ignore)]
        public static int ExistingMethod_Static()
        {
            Console.WriteLine("This is introduced static method.");
            return 42;
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
        public int ExistingMethod()
        {
            return 13;
        }

        public static int ExistingMethod_Static()
        {
            return 13;
        }
    }
}
