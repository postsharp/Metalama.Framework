using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.CodeModel.Cast
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var castNull = meta.Cast( meta.Method.ReturnType, null );
            var castParam = meta.Cast( meta.Method.ReturnType, (object?) meta.Parameters[0].Value );
            var castLiteral = meta.Cast( meta.Method.Parameters[1].ParameterType, 1 );
            
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