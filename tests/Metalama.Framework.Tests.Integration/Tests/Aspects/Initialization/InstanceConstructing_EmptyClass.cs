#if TESTOPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER
using System;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_EmptyClass
{
    public class Aspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            base.BuildAspect(builder);

            var constructor = builder.Target.Constructors.Single();
            builder.Advice.AddInitializer(constructor, StatementFactory.Parse("_ = 42;"));
        }
    }

    // <target>
    [Aspect]
    public class TargetCode;
}
#endif