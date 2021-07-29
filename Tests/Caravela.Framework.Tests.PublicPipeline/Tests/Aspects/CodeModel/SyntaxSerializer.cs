using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;


namespace Caravela.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer
{
    // Tests override method attribute where target method body contains return from the middle of the method. which forces aspect linker to use jumps to inline the override.
    // Template stores the result into a variable.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var methodInfo = meta.RunTime( meta.Method.ToMethodInfo() );
            var methodBase = meta.RunTime( meta.Method.ToMethodBase() );
            var memberInfo = meta.RunTime( meta.Method.ToMemberInfo() );
            var parameterInfo = meta.RunTime( meta.Method.Parameters[0].ToParameterInfo() );
            var returnValueInfo = meta.RunTime( meta.Method.ReturnParameter.ToParameterInfo() );
            var type = meta.RunTime( meta.Method.DeclaringType!.ToType() );
            var field = meta.Method.DeclaringType.Fields.Single().ToFieldOrPropertyInfo();
            var propertyAsFieldOrProperty = meta.Method.DeclaringType.Properties.Single().ToFieldOrPropertyInfo();
            var property = meta.RunTime( meta.Method.DeclaringType.Properties.Single().ToPropertyInfo() );
            var constructor = meta.RunTime( meta.Method.DeclaringType.Constructors.Single().ToConstructorInfo() );
            var constructorParameter = meta.RunTime( meta.Method.DeclaringType.Constructors.Single().Parameters.Single().ToParameterInfo() );
            
            return default;
        }
    }

    // <target>
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
