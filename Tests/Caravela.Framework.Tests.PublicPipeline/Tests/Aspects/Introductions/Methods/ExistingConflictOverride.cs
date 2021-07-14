using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverride
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int ExistingMethod()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static int ExistingMethod_Static()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int NonExistingMethod()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static int NonExistingMethod_Static()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }
    }

    internal class BaseClass
    {
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass
    {
        public int ExistingMethod()
        {
            return 27;
        }

        public static int ExistingMethod_Static()
        {
            return 27;
        }
    }
}
