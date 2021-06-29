using System.Collections.Generic;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.ForEachStatements.ForEachStatements
{
    class RunTimeClass
    {
        public IEnumerable<int> runTimeEnumerable;
    }

    [CompileTimeOnly]
    class CompileTimeClass
    {
        public IEnumerable<int> compileTimeEnumerable;
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
dynamic Template() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
}
