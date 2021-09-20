using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingConflictOverride
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int ExistingBaseProperty
        {
            get
            {
                Console.WriteLine("This is introduced property.");
                return meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int ExistingProperty
        {
            get
            {
                Console.WriteLine("This is introduced property.");
                return meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static int ExistingProperty_Static
        {
            get
            {
                Console.WriteLine("This is introduced property.");
                return meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int NotExistingProperty
        {
            get
            {
                Console.WriteLine("This is introduced property.");
                return meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static int NotExistingProperty_Static
        {
            get
            {
                Console.WriteLine("This is introduced property.");
                return meta.Proceed();
            }
        }
    }

    internal class BaseClass
    {
        public virtual int ExistingBaseProperty
        {
            get => 27;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass
    {
        public int ExistingProperty
        {
            get => 27;
        }

        public static int ExistingProperty_Static
        {
            get => 27;
        }
    }
}
