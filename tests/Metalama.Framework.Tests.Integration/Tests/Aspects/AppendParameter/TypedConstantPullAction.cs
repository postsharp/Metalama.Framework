using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.TypedConstantPullAction;

public class AddParameter : ConstructorAspect
{
    public override void BuildAspect(IAspectBuilder<IConstructor> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.IntroduceParameter(
            builder.Target, "arg", typeof(int), TypedConstant.Default(typeof(int)), (param, ctor) => PullAction.UseExpression(TypedConstant.Create(42)));
    }
}

// <target>
internal class TargetCode
{
    [AddParameter]
    TargetCode(string s) { }
    TargetCode(int i) : this(i.ToString()) { }
}