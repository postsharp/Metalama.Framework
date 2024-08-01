using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.IntegrationTests.Aspects.CodeModel.Default
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var classDefault = ExpressionFactory.Default( meta.Target.Method.ReturnType );
            var literalDefault = ExpressionFactory.Default( ( (IParameter)meta.Target.Method.Parameters[1] ).Type );
            var structDefault = ExpressionFactory.Default(( (IParameter)meta.Target.Method.Parameters[2] ).Type );

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