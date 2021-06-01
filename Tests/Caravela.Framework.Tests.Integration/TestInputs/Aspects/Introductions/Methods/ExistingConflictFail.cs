using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictFail
{
    // TODO: Will be fixed as part of #28322 Handle conflicts and overrides.

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
       
        
        
       

        [Introduce(ConflictBehavior = ConflictBehavior.Fail)]
        public int ExistingMethod()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(ConflictBehavior = ConflictBehavior.Fail)]
        public static int ExistingMethod_Static()
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
            return 13;
        }

        public static int ExistingMethod_Static()
        {
            return 13;
        }
    }
}
