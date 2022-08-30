using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.CopyAttributeToIntroducedMethod;


[assembly: AspectOrder( typeof(OverrideAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.CopyAttributeToIntroducedMethod
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        [Override]
        public void IntroducedMethod_Void()
        {
            Console.WriteLine( "This is introduced method." );
        }

        [Introduce]
        [Override]
        public int IntroducedMethod_Int()
        {
            Console.WriteLine( "This is introduced method." );

            return 42;
        }

        [Introduce]
        [Override]
        public int IntroducedMethod_Param( int x )
        {
            Console.WriteLine( $"This is introduced method, x = {x}." );

            return x;
        }

        [Introduce]
        [Override]
        public static int IntroducedMethod_StaticSignature()
        {
            Console.WriteLine( "This is introduced method." );

            return 42;
        }

        [Introduce( IsVirtual = true )]
        [Override]
        public int IntroducedMethod_VirtualExplicit()
        {
            Console.WriteLine( "This is introduced method." );

            return 42;
        }
    }

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Start.");

            try
            {
                var result = meta.Proceed();

                Console.WriteLine("Try.");

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(" Catch: " + e.Message);

                throw;
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        [Override]
        public int DefaultMethod()
        {
            Console.WriteLine("This is original method.");

            return 0;
        }
    }
}