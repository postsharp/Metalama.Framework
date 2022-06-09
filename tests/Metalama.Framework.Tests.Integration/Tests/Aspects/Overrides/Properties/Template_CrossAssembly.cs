using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.Template_CrossAssembly
{
    // <target>
    [TestAspect]
    internal class TargetClass
    {
        public string Property
        {
            get
            {
                Console.WriteLine("Aspect code");
                return "Test";
            }

            set
            {
                Console.WriteLine("Aspect code");
            }
        }

        public string ExpressionProperty => "Test";

        public string? AutoProperty { get; set; }

        public string AutoPropertyWithInitializer { get; set; } = "Test";
    }
}
