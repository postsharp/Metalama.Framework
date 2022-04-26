#if TEST_OPTIONS
// @Skipped(#29134 - Invokers.Base is null for an override aspect applied to a field)
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedIntroduction_FinalInvoker;

[assembly: AspectOrder( typeof(TestAttribute), typeof(TestIntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedIntroduction_FinalInvoker
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
            builder.Advice.Override( builder.Target.Methods.OfName( nameof(TestIntroductionAttribute.Method) ).Single(), nameof(MethodTemplate) );
        }

        [Template]
        public dynamic MethodTemplate()
        {
            return meta.Target.Method.Invokers.Final!.Invoke( meta.This, meta.Target.Method.Parameters[0].Value );
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass { }
}