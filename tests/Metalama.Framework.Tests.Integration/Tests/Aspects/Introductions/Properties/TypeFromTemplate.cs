using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.TypeFromTemplate
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceProperty(
                builder.Target, 
                "IntroducedProperty", 
                nameof(GetTemplate), 
                nameof(SetTemplate),
                args: new { x = "42" });

            builder.Advice.IntroduceProperty(
                builder.Target,
                "IntroducedProperty_GetOnly",
                nameof(GetTemplate),
                null,
                args: new { x = "42" });

            builder.Advice.IntroduceProperty(
                builder.Target,
                "IntroducedProperty_SetOnly",
                null,
                nameof(SetTemplate),
                args: new { x = "42" });
        }

        [Template]
        public int GetTemplate([CompileTime] string x )
        {
            return x.Length;
        }

        [Template]
        public void SetTemplate([CompileTime] string x, int y)
        {
            y = x.Length;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}