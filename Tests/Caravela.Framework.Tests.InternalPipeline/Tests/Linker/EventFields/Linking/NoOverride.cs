using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Linking.NoOverride
{
    // <target>
    class Target
    {
        [PseudoIntroduction("TestAspect")]
        [PseudoNotInlineable]
        public event EventHandler? Bar;
    }
}
