using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer_Introduced;

[assembly: AspectOrder(AspectOrderDirection.CompileTime, typeof(IntroductionAttribute), typeof(OverrideAttribute))]

namespace Metalama.Framework.IntegrationTests.Aspects.CodeModel.SyntaxSerializer_Introduced
{
    // Tests syntax serialization of introduced code model objects to reflection types.

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var introducedType = builder.IntroduceClass("IntroducedType");

            introducedType.IntroduceField("Field", typeof(int));
            introducedType.IntroduceAutomaticProperty("Property", typeof(int));

            introducedType.IntroduceConstructor(
                nameof(MethodTemplate),
                buildConstructor: m =>
                {
                    m.AddParameter("x", typeof(int));
                });

            introducedType.IntroduceMethod(
                nameof(MethodTemplate),
                buildMethod: m =>
                {
                    m.Name = "TargetMethod_Void";
                    m.AddParameter("x", typeof(int));
                });

            introducedType.IntroduceMethod(
                nameof(MethodTemplate),
                buildMethod: m =>
                {
                    m.Name = "TargetMethod_Int";
                    m.ReturnType = TypeFactory.GetType(typeof(int));
                    m.AddParameter("x", typeof(int));
                });

            builder.Outbound.SelectMany(t => t.NestedTypes).SelectMany(t => t.Methods.OfName("TargetMethod_Void")).AddAspect<OverrideAttribute>();
        }

        [Template]
        public dynamic? MethodTemplate()
        {
            return meta.Proceed();
        }
    }

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var methodInfo = meta.RunTime( meta.Target.Method.ToMethodInfo() );
            var methodBase = meta.RunTime( meta.Target.Method.ToMethodBase() );
            var memberInfo = meta.RunTime( meta.Target.Method.ToMemberInfo() );
            var parameterInfo = meta.RunTime( meta.Target.Method.Parameters[0].ToParameterInfo() );
            var returnValueInfo = meta.RunTime( meta.Target.Method.ReturnParameter.ToParameterInfo() );
            var type = meta.RunTime( meta.Target.Method.DeclaringType!.ToType() );
            var field = meta.Target.Method.DeclaringType.Fields.Single( f => !f.IsImplicitlyDeclared ).ToFieldOrPropertyInfo();
            var propertyAsFieldOrProperty = meta.Target.Method.DeclaringType.Properties.Single().ToFieldOrPropertyInfo();
            var property = meta.RunTime( meta.Target.Method.DeclaringType.Properties.Single().ToPropertyInfo() );
            var constructor = meta.RunTime( meta.Target.Method.DeclaringType.Constructors.Single().ToConstructorInfo() );
            var constructorParameter = meta.RunTime( meta.Target.Method.DeclaringType.Constructors.Single().Parameters.Single().ToParameterInfo() );
            var array = meta.RunTime( new MemberInfo[] { meta.Target.Method.ToMethodInfo(), meta.Target.Type.ToType() } );

            return default;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
    }
}