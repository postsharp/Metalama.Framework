using Caravela.Framework.Aspects;
using Caravela.Framework.Code.SyntaxBuilders;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Tests.Templating.InvalidCode.InvalidLiteralInInvocation
{
   
    public class EnrichExceptionAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var methodSignatureBuilder = new InterpolatedStringBuilder();
            
            // The next line has an intentional syntax error.
            methodSignatureBuilder.AddText(""(');
            
            return meta.Proceed();
        }
    }
}