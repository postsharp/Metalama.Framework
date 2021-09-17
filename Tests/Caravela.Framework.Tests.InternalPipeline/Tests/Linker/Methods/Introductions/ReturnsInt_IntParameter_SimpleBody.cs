using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Introductions.ReturnsInt_IntParameter_SimpleBody
{
    // <target>
    class Target
    {
        [PseudoIntroduction("TestAspect")]
        public int Foo(int x)
        {
            return 42;
        }

        [PseudoIntroduction("TestAspect")]
        public static int Bar(int x)
        {
            return 42;
        }
    }
}
