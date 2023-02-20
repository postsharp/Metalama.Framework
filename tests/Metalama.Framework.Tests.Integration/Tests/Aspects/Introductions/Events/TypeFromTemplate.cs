using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Events.TypeFromTemplate
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceEvent(
                builder.Target, 
                "IntroducedEvent", 
                nameof(Template), 
                nameof(Template),
                args: new { x = "42" });
        }

        [Template]
        public void Template([CompileTime] string x, EventHandler y)
        {
            y.Invoke(null, new EventArgs());
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}