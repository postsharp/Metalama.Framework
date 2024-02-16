using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Order;

#pragma warning disable CS8618

[assembly: AspectOrder(typeof(Test1Attribute), typeof(Test2Attribute), typeof(Test3Attribute))]

/*
 * Tests that multiple contract aspects are ordered correctly, and this order is kept when override is placed in between.
 */

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Order
{
    internal class Test1Attribute : ParameterAspect
    {
        public override void BuildAspect(IAspectBuilder<IParameter> builder)
        {
            builder.Advice.AddContract(builder.Target, nameof(Validate), args: new { order = 1 });
            builder.Advice.AddContract(builder.Target, nameof(Validate), args: new { order = 2 });
        }

        [Template]
        public void Validate(dynamic? value, [CompileTime] int order)
        {
            Console.WriteLine($"Contract by aspect 1 on {meta.Target.Parameter}, ordinal {order}");
        }
    }
    internal class Test2Attribute : ParameterAspect
    {
        public override void BuildAspect(IAspectBuilder<IParameter> builder)
        {
            builder.Advice.AddContract(builder.Target, nameof(Validate), args: new { order = 1 });
            builder.Advice.AddContract(builder.Target, nameof(Validate), args: new { order = 2 });
            builder.Advice.Override((IMethod)builder.Target.ContainingDeclaration, nameof(Template), args: new { parameter = builder.Target });
            builder.Advice.AddContract(builder.Target, nameof(Validate), args: new { order = 3 });
            builder.Advice.AddContract(builder.Target, nameof(Validate), args: new { order = 4 });
        }

        [Template]
        public void Validate(dynamic? value, [CompileTime] int order)
        {
            Console.WriteLine($"Contract by aspect 2 on {meta.Target.Parameter}, ordinal {order}");
        }

        [Template]
        public dynamic? Template([CompileTime] IParameter parameter)
        {
            Console.WriteLine($"Override by aspect 2 on {parameter}");
            return meta.Proceed();
        }
    }
    internal class Test3Attribute : ParameterAspect
    {
        public override void BuildAspect(IAspectBuilder<IParameter> builder)
        {
            builder.Advice.AddContract(builder.Target, nameof(Validate), args: new { order = 1 });
            builder.Advice.AddContract(builder.Target, nameof(Validate), args: new { order = 2 });
        }

        [Template]
        public void Validate(dynamic? value, [CompileTime] int order)
        {
            Console.WriteLine($"Contract by aspect 3 on {meta.Target.Parameter}, ordinal {order}");
        }
    }

    // <target>
    internal class Target
    {
        private void Override([Test1][Test2][Test3] int p1, [Test1][Test2][Test3] int p2) { }

        private void NoOverride([Test1][Test3] int p1, [Test1][Test3] int p2) { }
    }
}