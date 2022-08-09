using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
