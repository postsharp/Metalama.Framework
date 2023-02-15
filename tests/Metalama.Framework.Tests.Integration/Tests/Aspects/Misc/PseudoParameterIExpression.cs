using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.IntegrationTests.Aspects.Misc.PseudoParameterIExpression;

public class TestAttribute : OverrideFieldOrPropertyAspect
{
    public override dynamic? OverrideProperty
    {
        get => meta.Proceed();
        set
        {
            var eb = new ExpressionBuilder();
            eb.AppendExpression(meta.Target.Parameters.Single());
            Console.WriteLine(eb.ToValue());
        }
    }
}

// <target>
internal class TargetClass
{
    [Test]
    public int Field;
}