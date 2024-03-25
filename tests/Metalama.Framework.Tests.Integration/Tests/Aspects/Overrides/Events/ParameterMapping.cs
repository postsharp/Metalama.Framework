using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Events.ParameterMapping
{
    /*
     * Verifies that template parameter is correctly mapped by index.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.OverrideAccessors(
                builder.Target.Events.Single(),
                nameof(RenamedValueParameter),
                nameof(RenamedValueParameter));
        }

        [Template]
        public void RenamedValueParameter(EventHandler x)
        {
            x.Invoke(null, new EventArgs());
            meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass 
    {
        public event EventHandler Event
        {
            add
            {
                EventHandler ev = value;
            }

            remove
            {
                EventHandler ev = value;
            }
        }
    }
}