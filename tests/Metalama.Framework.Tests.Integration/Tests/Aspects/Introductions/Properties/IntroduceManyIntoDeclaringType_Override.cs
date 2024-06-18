using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Aspects.Introductions.Properties.IntroduceManyIntoDeclaringType_Override
{
    internal class Aspect : MethodAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override )]

        private string Property
        {
            get
            {
                Console.WriteLine( $"{meta.AspectInstance.TargetDeclaration} says hello." );

                return meta.Proceed()!;
            }
        }
    }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private void M() { }

        [Aspect]
        private void M2() { }
    }
}