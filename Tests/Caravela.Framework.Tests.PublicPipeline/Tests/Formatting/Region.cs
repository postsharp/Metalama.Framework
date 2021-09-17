using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.Region
{
    // Only the <aspect> region should be included in the output.
    
    // <aspect>
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            throw new System.NotImplementedException();
        }
    }
    // </aspect>

}
