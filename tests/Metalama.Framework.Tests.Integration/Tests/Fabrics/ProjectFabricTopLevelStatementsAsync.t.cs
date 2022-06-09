// --- ProjectFabricTopLevelStatementsAsync.cs ---

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Fabrics.ProjectFabricTopLevelStatementsAsync
{
#pragma warning disable CS0067
    /*
     * Tests that applying an override on all methods does not target the async Main method containing top-level statements.
     */

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }

#pragma warning restore CS0067
#pragma warning disable CS0067

    public class MyProjectFabric : ProjectFabric
    {
        public override void AmendProject(IProjectAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }

#pragma warning restore CS0067
}
    
    // --- __TopLevelStatementsAsync.cs ---
    
    using System;
    using System.Threading.Tasks;
    
    Console.WriteLine("TopLevelStatement1");

await Task.Yield();

Console.WriteLine("TopLevelStatement2");