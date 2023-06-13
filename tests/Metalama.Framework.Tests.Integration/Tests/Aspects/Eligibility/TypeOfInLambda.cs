using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Aspects.Eligibility.TypeOfInLambda;

class TestAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
    }

    public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
    {
        builder.MustSatisfy(
            method => method.Attributes.Any(a => a.ConstructorArguments is { Length: 1 } && a.ConstructorArguments.Single().Value as string == typeof(RunTimeClass).Name),
            method => $"{method} must have a an attribute with {typeof(RunTimeClass)} argument");

        builder.MustSatisfy(
            method => method.Attributes.Any(a => a.ConstructorArguments is { Length: 1 } && a.ConstructorArguments.Single().Value as string == typeof(RunTimeOrCompileTimeClass).Name),
            method => $"{method} must have a an attribute with {typeof(RunTimeOrCompileTimeClass)} argument");

        builder.MustSatisfy(
            method => method.Attributes.Any(a => a.ConstructorArguments is { Length: 1 } && a.ConstructorArguments.Single().Value as string == typeof(CompileTimeClass).Name),
            method => $"{method} must have a an attribute with {typeof(CompileTimeClass)} argument");
    }
}

class RunTimeClass { }

[RunTimeOrCompileTime]
class RunTimeOrCompileTimeClass { }

[CompileTime]
class CompileTimeClass { }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
class TypeAttribute : Attribute
{
    public TypeAttribute(string type) { }
}

// <target>
public partial class TargetClass
{
    [TestAspect]
    [Type(nameof(RunTimeClass))]
    [Type(nameof(RunTimeOrCompileTimeClass))]
    [Type(nameof(CompileTimeClass))]
    void M1() { }

    [TestAspect]
    [Type(nameof(RunTimeClass))]
    [Type(nameof(RunTimeOrCompileTimeClass))]
    void M2() { }

    [TestAspect]
    [Type(nameof(RunTimeClass))]
    void M3() { }

    [TestAspect]
    void M4() { }
}