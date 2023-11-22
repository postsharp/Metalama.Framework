using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.EventFields.Template_CrossAssembly
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var @event in builder.Target.Events)
            {
                builder.Advice.OverrideAccessors( @event, nameof(Override), nameof(Override), null );
            }
        }

        [Template]
        public dynamic? Override()
        {
            Console.WriteLine( "Aspect code" );

            return meta.Proceed();
        }
    }
}