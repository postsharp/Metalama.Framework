using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverride
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int ExistingBaseMethod()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int ExistingMethod()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public static int ExistingMethod_Static()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int NotExistingMethod()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public static int NotExistingMethod_Static()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public virtual int ExistingBaseMethod()
        {
            return 27;
        }
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