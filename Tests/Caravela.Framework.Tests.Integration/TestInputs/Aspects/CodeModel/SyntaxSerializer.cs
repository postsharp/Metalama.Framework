using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using meta = Caravela.Framework.Aspects.meta;

namespace Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer
{
    // Tests override method attribute where target method body contains return from the middle of the method. which forces aspect linker to use jumps to inline the override.
    // Template stores the result into a variable.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var methodInfo = meta.Method.ToMethodInfo();
            var methodBase = meta.Method.ToMethodBase();
            var memberInfo = meta.Method.ToMemberInfo();
            var parameterInfo = meta.Method.Parameters[0].ToParameterInfo();
            var returnValueInfo = meta.Method.ReturnParameter.ToParameterInfo();
            var type = meta.Method.DeclaringType.ToType();
           // var field = target.Method.DeclaringType.Fields.Single().ToFieldOrPropertyInfo();
            var propertyAsFieldOrProperty = meta.Method.DeclaringType.Properties.Single().ToFieldOrPropertyInfo();
            var property = meta.Method.DeclaringType.Properties.Single().ToPropertyInfo();
            var constructor = meta.Method.DeclaringType.Constructors.Single().ToConstructorInfo();
            var constructorParameter = meta.Method.DeclaringType.Constructors.Single().Parameters.Single().ToParameterInfo();
            
             
            
            return default;
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        public TargetClass( int x ) { }

        public int Field;
        public int Property { get; set; }
        
        [Override]
        public void TargetMethod_Void(int x)
        {
           
        }

        public int TargetMethod_Int(int x)
        {
           return 0;
        }
    }
}
