﻿using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.Tests.Linker.EventFields.Linking.NoOverride
{
    // <target>
    class Target
    {
        [PseudoIntroduction("TestAspect")]
        [PseudoNotInlineable]
        public event EventHandler? Bar;
    }
}
