using System.Reflection;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.MemberInfoAsIExpression_Runtime;

public sealed class TestAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        // typeof(RunTimeOrCompileTimeClass) is RuntimeType, not CompileTimeType, so GetMethod works on it (and returns RuntimeMethodInfo).
        var method = typeof(RunTimeOrCompileTimeClass).GetMethod("M");

        var arrayBuilder = new ArrayBuilder(typeof(MethodInfo));
        arrayBuilder.Add(method);

        var methodsInvalidatedByField = builder.Advice.IntroduceField(
            builder.Target,
            "methods",
            typeof(MethodInfo[]),
            buildField: b =>
            {
                b.InitializerExpression = arrayBuilder.ToExpression();
            });
    }
}

// <target>
[TestAspect]
internal class TargetCode
{
}

[RunTimeOrCompileTime]
class RunTimeOrCompileTimeClass
{
    public void M() { }
}