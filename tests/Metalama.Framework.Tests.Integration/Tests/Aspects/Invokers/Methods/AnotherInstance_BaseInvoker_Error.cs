using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherInstance_BaseInvoker_Error
{
    [AttributeUsage( AttributeTargets.Class )]
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            var overrideBuilder = aspectBuilder.Advice.IntroduceMethod( aspectBuilder.Target, nameof(OverrideMethod), whenExists: OverrideStrategy.Override );
            overrideBuilder.Name = "Foo";
            overrideBuilder.ReturnType = TypeFactory.GetType( SpecialType.Void );
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            var x = meta.This;

            return meta.Target.Method.Invokers.Base!.Invoke( x );
        }
    }

    internal class BaseClass
    {
        public virtual void Foo() { }
    }

    // <target>
    [TestAttribute]
    internal class TargetClass : BaseClass { }
}