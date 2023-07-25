using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Aspects.RecordAspectOrdering;

internal class Fabric : ProjectFabric
{
    public override void AmendProject(IProjectAmender amender)
        => amender.Outbound
            .SelectMany(compilation => compilation.AllTypes)
            .SelectMany(type => type.Methods)
            .AddAspectIfEligible<LogAttribute>();
}

public class LogAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine($"{meta.Target.Method} started.");

        return meta.Proceed();
    }

    public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
    {
        // Don't call base to skip MustBeExplicitlyDeclared rule.
    }
}

// <target>
public record Person(string Name)
{
    public Guid Id { get; init; }
}