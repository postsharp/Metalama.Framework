using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30976;

/*
 * Tests that MyAspect is wrapped with `#pragma warning`, but it does
not interfere with the #if/#endif directives above the aspect.
 */

#if YES

#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414

public class MyAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}
#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414


#endif

public class X { }

