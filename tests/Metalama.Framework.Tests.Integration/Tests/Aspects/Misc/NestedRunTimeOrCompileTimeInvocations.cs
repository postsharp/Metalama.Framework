using System;
using System.Linq.Expressions;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.NestedRunTimeOrCompileTimeInvocations;

public class AspectAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Expression.Property(Expression.Parameter(typeof(int), "p"), "propertyName");
        var p = Expression.Property(Expression.Parameter(typeof(int), "p"), "propertyName");
        return Expression.Property(Expression.Parameter(typeof(int), "p"), "propertyName");
    }
}

// <target>
internal class TargetCode
{
    [Aspect]
    private Expression? M() => null;
}