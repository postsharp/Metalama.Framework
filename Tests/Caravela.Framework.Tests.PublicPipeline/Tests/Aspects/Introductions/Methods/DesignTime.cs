using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.DesignTime
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public void IntroducedMethod_Void()
        {
            Console.WriteLine( "This is introduced method." );
            var nic = meta.Proceed();
        }

        [Introduce]
        public int IntroducedMethod_Int()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }

        [Introduce]
        public int IntroducedMethod_Param( int x )
        {
            Console.WriteLine( $"This is introduced method, x = {x}." );

            return meta.Proceed();
        }

        [Introduce]
        public static int IntroducedMethod_StaticSignature()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }

        [Introduce( IsVirtual = true )]
        public int IntroducedMethod_VirtualExplicit()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal partial class TargetClass { }
}