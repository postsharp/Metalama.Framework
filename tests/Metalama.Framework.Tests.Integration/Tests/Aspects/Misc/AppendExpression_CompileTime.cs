using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.AppendExpression_CompileTime;

class TestAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        meta.InsertStatement(CreateConsoleWriteLine(meta.Target.Parameters.Single()));

        return meta.Proceed();
    }

    [CompileTime]
    private static IExpression CreateConsoleWriteLine(IParameter parameter)
    {
        var builder = new ExpressionBuilder();

        builder.AppendTypeName(typeof(Console));
        builder.AppendVerbatim(".WriteLine(\"{0},{1},{2}\", ");
        builder.AppendExpression(parameter.Value); // fine
        builder.AppendVerbatim(",");
        builder.AppendExpression(42); // fine
        builder.AppendVerbatim(",");
        builder.AppendExpression(new Exception()); // exception
        builder.AppendVerbatim(")");

        return builder.ToExpression();
    }
}

// <target>
class TargetClass
{
    [TestAspect]
    void M(object obj) { }
}
