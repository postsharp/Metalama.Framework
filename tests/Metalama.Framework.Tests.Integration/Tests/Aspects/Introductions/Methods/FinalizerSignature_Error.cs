using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.FinalizerSignature_Error
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var methodBuilder = builder.Advice.IntroduceMethod(builder.Target, nameof(Template));

            methodBuilder.Name = "Finalize";
        }

        [Template]
        public void Template()
        {
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}