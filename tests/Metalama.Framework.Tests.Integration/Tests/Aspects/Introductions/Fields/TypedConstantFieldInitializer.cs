using System;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.TypedConstantFieldInitializer;

public class AddField : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.IntroduceField(
            builder.Target, "F", typeof(int), buildField: fieldBuilder => fieldBuilder.InitializerExpression = TypedConstant.Create(42));
    }
}

// <target>
[AddField]
internal class TargetCode { }