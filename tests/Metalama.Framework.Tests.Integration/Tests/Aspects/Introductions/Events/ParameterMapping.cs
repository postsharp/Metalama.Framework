using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Events.ParameterMapping
{
    /*
     * Verifies that template parameter is correctly mapped to "value".
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceEvent(
                builder.Target,
                "Event",
                nameof(Template),
                nameof(Template) );
        }

        [Template]
        public void Template(EventHandler x)
        {
            var z = x;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}