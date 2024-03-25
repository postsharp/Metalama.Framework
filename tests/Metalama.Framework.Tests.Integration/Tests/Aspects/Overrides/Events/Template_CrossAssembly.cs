using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Events.Template_CrossAssembly
{
    // <target>
    [TestAspect]
    internal class TargetClass
    {
        public event EventHandler Event
        {
            add
            {
                Console.WriteLine("Original code");
            }

            remove
            {
                Console.WriteLine("Original code");
            }
        }
    }
}
