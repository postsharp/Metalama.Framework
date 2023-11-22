using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.EventFields.Template_CrossAssembly
{
    // <target>
    [TestAspect]
    internal class TargetClass
    {
        public event EventHandler? Event;
    }
}
