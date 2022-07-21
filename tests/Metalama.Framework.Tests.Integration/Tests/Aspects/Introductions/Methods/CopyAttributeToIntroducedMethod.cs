using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.CopyAttributeToIntroducedMethod;


[assembly: AspectOrder( typeof(CopiedLogAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.CopyAttributeToIntroducedMethod
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        [CopiedLog]
        public void IntroducedMethod_Void()
        {
            Console.WriteLine( "This is introduced method." );
            meta.Proceed();
        }

        [Introduce]
        [CopiedLog]
        public int IntroducedMethod_Int()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }

        [Introduce]
        [CopiedLog]
        public int IntroducedMethod_Param( int x )
        {
            Console.WriteLine( $"This is introduced method, x = {x}." );

            return meta.Proceed();
        }

        [Introduce]
        [CopiedLog]
        public static int IntroducedMethod_StaticSignature()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }

        [Introduce( IsVirtual = true )]
        [CopiedLog]
        public int IntroducedMethod_VirtualExplicit()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }
    }

    public class CopiedLogAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(meta.Target.Method.ToDisplayString() + " started.");

            try
            {
                var result = meta.Proceed();

                Console.WriteLine(meta.Target.Method.ToDisplayString() + " succeeded.");

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(meta.Target.Method.ToDisplayString() + " failed: " + e.Message);

                throw;
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        [CopiedLog]
        public int DefaultMethod()
        {
            return 0;
        }
    }
}