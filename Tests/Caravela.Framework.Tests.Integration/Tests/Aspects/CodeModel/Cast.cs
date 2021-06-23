using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.CodeModel.Cast
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var castNull = meta.Method.ReturnType.Cast( null );
            var castParam = meta.Method.ReturnType.Cast( (object) meta.Parameters[0].Value );
            var castLiteral = meta.Method.Parameters[1].ParameterType.Cast( 1 );
            
            return default;
        }
    }
    
    // <target>
    internal class TargetClass
    {
       
        [Override]
        public TargetClass? TargetMethod_Void(object o, decimal d)
        {
            return null;
        }
    }
    
}