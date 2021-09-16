using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Introductions.ReturnsVoid_NoParameter_SimpleBody
{
    // <target>
    class Target
    {
        [PseudoIntroduction("TestAspect")]
        public void Foo()
        {
        }
    }
}
