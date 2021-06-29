using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.CodeModel.Default
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var classDefault = meta.Method.ReturnType.DefaultValue();
            var literalDefault = meta.Method.Parameters[1].ParameterType.DefaultValue();
            var structDefault = meta.Method.Parameters[2].ParameterType.DefaultValue();
            
            return default;
        }
    }
    
    // <target>
    internal class TargetClass
    {
       
        [Override]
        public TargetClass? TargetMethod_Void(object o, decimal d, St s)
        {
            return null;
        }
    }

    struct St
    {
        
    }
    
}