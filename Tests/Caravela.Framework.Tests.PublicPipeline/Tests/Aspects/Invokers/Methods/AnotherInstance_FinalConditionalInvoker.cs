// @Skipped #28907 Linker: conditional access expression

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherInstance_FinalConditionalInvoker
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            var overrideBuilder = aspectBuilder.AdviceFactory.IntroduceMethod(aspectBuilder.TargetDeclaration, nameof(OverrideMethod), whenExists: OverrideStrategy.Override);
            overrideBuilder.Name = "Foo";
            overrideBuilder.ReturnType = aspectBuilder.TargetDeclaration.Compilation.TypeFactory.GetSpecialType(SpecialType.Void);
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            var x = meta.This;
            return meta.Method.Invokers.FinalConditional.Invoke(x);
        }
    }

    // <target>
    [TestAttribute]
    internal class TargetClass
    {
        public void Foo()
        {
        }
    }
}
