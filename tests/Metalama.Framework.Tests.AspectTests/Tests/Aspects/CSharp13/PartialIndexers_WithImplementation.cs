#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_12_0_OR_GREATER)
#endif

#if ROSLYN_4_12_0_OR_GREATER

using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;

namespace Metalama.Framework.Tests.AspectTests.Tests.Aspects.CSharp13.PartialIndexers_WithImplementation;

public class TheAspect : Aspect, IAspect<IIndexer>
{
    public void BuildEligibility(IEligibilityBuilder<IIndexer> builder)
    {
    }

    public void BuildAspect(IAspectBuilder<IIndexer> builder)
    {
        builder.OverrideAccessors(nameof(GetterTemplate), nameof(SetterTemplate));
    }

    [Template]
    dynamic? GetterTemplate(dynamic index)
    {
        Console.WriteLine("This is aspect code.");

        return meta.Proceed();
    }

    [Template]
    void SetterTemplate(dynamic index, dynamic value)
    {
        Console.WriteLine("This is aspect code.");

        meta.Proceed();
    }
}

// <target>
partial class Target
{
    [TheAspect]
    partial int this[int i] { get; set; }

    partial int this[int i] { get => 0; set => throw new Exception(); }

    partial int this[string s] { get; set; }

    [TheAspect]
    partial int this[string s] { get => 0; set => throw new Exception(); }

    [TheAspect]
    partial int this[long i] { get; }

    partial int this[long i] { get => 0; }
}

#endif