using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.DynamicInvoke
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public void IntroduceVoid()
        {
            Console.WriteLine( "Introduced" );
            meta.Target.Method.Invoke();
        }

        [Introduce]
        public int IntroduceInt()
        {
            Console.WriteLine( "Introduced" );

            return meta.Target.Method.Invoke();
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int OverrideInt()
        {
            Console.WriteLine( "Introduced" );

            // TODO: This produces an incorrect result.
            return meta.Target.Method.Invoke();
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public void OverrideVoid()
        {
            Console.WriteLine( "Introduced" );

            // TODO: This produces an incorrect result.
            meta.Target.Method.Invoke();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public int OverrideInt()
        {
            return 1;
        }

        public void OverrideVoid() { }
    }
}