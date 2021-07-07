using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.Declarations.RunTimeVariableDeclarations
{
    class RuntimeClass
    {
        public void RunTimeMethod()
        {
        }
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
dynamic? Template() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
}
