using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.RefParameter_Both;
#pragma warning disable CS0067

internal class NotNullAttribute : FilterAspect
{
    public override void Filter(dynamic? value) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}
#pragma warning restore CS0067

internal class Target
{
    private void M( [NotNull(Direction = FilterDirection.Both)] ref string m )
    {
    if (m == null)
    {
        throw new global::System.ArgumentNullException("m");
    }

            m = "";
    if (m == null)
    {
        throw new global::System.ArgumentNullException("m");
    }
    }
}
