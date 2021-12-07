using System;
using System.Collections.Generic;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Introductions.Methods.GenericBaseType
{
    internal class Aspect : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.New )]
        public void Add( int value )
        {
            Console.WriteLine( "Oops" );
            meta.Proceed();
        }
    }

    // <target>
    [Aspect]
    internal class TargetCode : List<int> { }
}