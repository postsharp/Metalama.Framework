using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Accessibility_CrossAssembly
{
    /*
     * Tests accessibility of implicit members in cross assembly scenario.
     */

    // <target>
    [Introduction]
    public class TargetClass { }
}