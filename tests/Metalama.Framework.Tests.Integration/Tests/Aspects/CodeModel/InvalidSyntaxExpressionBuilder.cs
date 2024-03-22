using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.CodeModel.InvalidSyntaxExpressionBuilder;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var eb = new ExpressionBuilder();

        if (meta.Target.Parameters.Count > 0)
        {
            var i = meta.CompileTime(0);
            foreach (IParameter parameter in meta.Target.Parameters)
            {
                if (i > 0) eb.AppendVerbatim(", ");
                eb.AppendExpression(parameter);
                i++;
            }

            Console.WriteLine(eb.ToValue());
        }

        return meta.Proceed();
    }
}

class TargetCode
{
    // <target>
    [Aspect]
    int Method(int a, int b)
    {
        return a;
    }
}