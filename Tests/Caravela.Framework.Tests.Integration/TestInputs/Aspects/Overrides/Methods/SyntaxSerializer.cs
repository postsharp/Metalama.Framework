using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.SyntaxSerializer
{
    // Tests override method attribute where target method body contains return from the middle of the method. which forces aspect linker to use jumps to inline the override.
    // Template stores the result into a variable.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            var methodInfo = target.Method.ToMethodInfo();
            var methodBase = target.Method.ToMethodBase();
            var memberInfo = target.Method.ToMemberInfo();
            var parameterInfo = target.Method.Parameters[0].ToParameterInfo();
            var returnValueInfo = target.Method.ReturnParameter.ToParameterInfo();
            
            return default;
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void(int x)
        {
           
        }

        [Override]
        public int TargetMethod_Int(int x)
        {
           return 0;
        }
    }
}
