using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverride
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

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public void ExistingBaseMethod_Void()
        {
            Console.WriteLine("This is introduced method.");

            meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public void ExistingMethod_Void()
        {
            Console.WriteLine("This is introduced method.");

            meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static void ExistingMethod_Void_Static()
        {
            Console.WriteLine("This is introduced method.");

            meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public void NotExistingMethod_Void()
        {
            Console.WriteLine("This is introduced method.");

            meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static void NotExistingMethod_Void_Static()
        {
            Console.WriteLine("This is introduced method.");

            meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public virtual int ExistingBaseMethod()
        {
            Console.WriteLine("This is existing base method.");
            return 27;
        }

        public virtual void ExistingBaseMethod_Void()
        {
            Console.WriteLine("This is existing base method.");
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass
    {
        public int ExistingMethod()
        {
            Console.WriteLine("This is existing method.");
            return 27;
        }

        public static int ExistingMethod_Static()
        {
            Console.WriteLine("This is existing method.");
            return 27;
        }

        public void ExistingMethod_Void()
        {
            Console.WriteLine("This is existing method.");
        }

        public static void ExistingMethod_Void_Static()
        {
            Console.WriteLine("This is existing method.");
        }
    }
}