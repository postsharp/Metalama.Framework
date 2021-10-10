using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedIntroduction_BaseInvoker;

[assembly: AspectOrder( typeof(TestAttribute), typeof(TestIntroductionAttribute) )]

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedIntroduction_BaseInvoker
{
    [AttributeUsage( AttributeTargets.Class )]
    public class TestIntroductionAttribute : TypeAspect
    {
        [Introduce]
        public int Method( int x )
        {
            return x;
        }
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.OverrideMethod( builder.Target.Methods.OfName( nameof(TestIntroductionAttribute.Method) ).Single(), nameof(MethodTemplate) );
        }

        [Template]
        public dynamic MethodTemplate()
        {
            return meta.Target.Method.Invokers.Base!.Invoke( meta.This, meta.Target.Method.Parameters[0].Value );
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass { }
}