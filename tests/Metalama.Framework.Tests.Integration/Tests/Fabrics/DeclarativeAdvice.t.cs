using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics.DeclarativeAdvice;
#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414

public class C
{
#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
    private class F : TypeFabric
    {
        [Introduce]
[global::Metalama.Framework.Aspects.AccessibilityAttribute(global::Metalama.Framework.Code.Accessibility.Private)]
public void M() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }

#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414


private void M()
{
}}
#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
