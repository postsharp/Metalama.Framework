using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.CodeModel.Default
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var classDefault = meta.Target.Method.ReturnType.DefaultValue();
            var literalDefault = ( (IParameter)meta.Target.Method.Parameters[1] ).Type.DefaultValue();
            var structDefault = ( (IParameter)meta.Target.Method.Parameters[2] ).Type.DefaultValue();

            return default;
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public TargetClass? TargetMethod_Void( object o, decimal d, St s )
        {
            return null;
        }
    }

    internal struct St { }
}