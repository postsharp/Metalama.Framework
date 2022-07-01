using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.Target_Record_Pull;

public class MyAspect : ConstructorAspect
{
    public override void BuildAspect(IAspectBuilder<IConstructor> builder)
    {
        builder.Advice.IntroduceParameter(builder.Target, "p", typeof(int), TypedConstant.Create(15),
            ( parameter, constructor ) => PullAction.IntroduceParameterAndPull( parameter.Name, parameter.Type, TypedConstant.Create(20) ) );
    }
}

public record R
{
    [MyAspect]
    public R() { }

    public R(string s) : this() { }
}

public record S : R
{

}