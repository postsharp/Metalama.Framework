// @RequiredConstant(NET6_0_OR_GREATER)

#if NET6_0_OR_GREATER

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp11.RefField_NotEligible;

public class TheAspect : OverrideFieldOrPropertyAspect
{
    public override dynamic? OverrideProperty { get; set; }
}

internal ref struct S
{
    [TheAspect]
    private ref int x;
}

#endif