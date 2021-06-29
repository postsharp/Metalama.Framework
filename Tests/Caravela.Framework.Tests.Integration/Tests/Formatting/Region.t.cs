using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.Region
{
    // <aspect>
    class RunTimeClass
    {
        public void RunTimeMethod()
        {
        }
    }
    // </aspect>

    [CompileTimeOnly]
    class CompileTimeClass
    {
        public void CompileTimeMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }

}
