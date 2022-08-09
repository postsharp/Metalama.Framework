
// --- ProjectFabricTopLevelStatements.cs ---

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Fabrics.ProjectFabricTopLevelStatements
{
#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
    /*
     * Tests that applying an override on all methods does not target the Main method containing top-level statements.
     */

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }

#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
#pragma warning disable CS0067, CS8618, CA1822, CS0162, CS0169, CS0414

    public class MyProjectFabric : ProjectFabric
    {
        public override void AmendProject(IProjectAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }

#pragma warning restore CS0067, CS8618, CA1822, CS0162, CS0169, CS0414
}

// --- __TopLevelStatements.cs ---

using System;

Console.WriteLine("TopLevelStatement");
