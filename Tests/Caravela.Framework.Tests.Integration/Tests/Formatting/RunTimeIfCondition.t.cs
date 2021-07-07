using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.RunTimeIfCondition
{
    class RunTimeClass
    {
        public void RunTimeMethod()
        {
        }
    }

    [CompileTimeOnly]
    class CompileTimeClass
    {
        public void CompileTimeMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
dynamic? Template() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
}
