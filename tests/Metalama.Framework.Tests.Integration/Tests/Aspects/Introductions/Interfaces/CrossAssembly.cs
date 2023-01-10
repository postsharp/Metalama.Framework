using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.CrossAssembly
{
    /*
     * Tests that interface is correctly introduced by aspect defined in another assembly.
     */

    // <target>
    [Introduction]
    public class TargetClass { }
}