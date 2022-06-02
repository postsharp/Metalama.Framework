using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.CrossAssemblyChildAspect
{
    public class C : I
    {
        public void M()
        {
            Console.WriteLine( "Hello, world." );
        }
    }
}