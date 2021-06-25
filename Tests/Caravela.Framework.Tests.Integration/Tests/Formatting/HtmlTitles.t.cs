using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.HtmlTitles
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
    class Aspect : OverrideMethodAspect, IAspect<INamedType>
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");


        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            
        }

        [Template]
dynamic Template() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
}
