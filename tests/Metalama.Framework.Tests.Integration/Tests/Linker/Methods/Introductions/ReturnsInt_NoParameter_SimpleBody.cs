﻿namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Introductions.ReturnsInt_NoParameter_SimpleBody
{
    // <target>
    class Target
    {
        [PseudoIntroduction("TestAspect")]
        public int Foo()
        {
            return 42;
        }

        [PseudoIntroduction("TestAspect")]
        public static int Bar()
        {
            return 42;
        }
    }
}
