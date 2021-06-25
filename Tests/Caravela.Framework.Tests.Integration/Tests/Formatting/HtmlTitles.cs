// @AddHtmlTitles

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
        public void CompileTimeMethod()
        {
        }
    }

    [CompileTime]
    class Aspect : OverrideMethodAspect, IAspect<INamedType>
    {
        public override dynamic? OverrideMethod()
        {
            throw new System.NotImplementedException();
        }

        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            
        }

        [Template]
        dynamic Template()
        {
            var m = meta.Method;
            var p = meta.Method.Parameters[0].Value;
            
            return meta.Proceed();
        }
    }
}
