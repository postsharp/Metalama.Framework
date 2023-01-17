using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Events.Template_CrossAssembly
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var @event in builder.Target.Events)
            {
                builder.Advise.OverrideAccessors(@event, nameof(Override), nameof(Override), null);
            }
        }

        [Template]
        public dynamic? Override()
        {
            Console.WriteLine("Aspect code");
            return meta.Proceed();
        }
    }
}