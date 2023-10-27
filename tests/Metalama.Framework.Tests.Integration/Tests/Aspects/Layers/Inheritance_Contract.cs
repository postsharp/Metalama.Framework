using System;
using System.Diagnostics;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Layers.Inheritance_Contract
{
    [Layers("test")]
    public class DerivedAspect : ContractAspect
    {
        public override void BuildAspect(IAspectBuilder<IFieldOrPropertyOrIndexer> builder)
        {
            // TODO: Why is this not executed??
            builder.Advice.Override(builder.Target.GetMethod, nameof(OverrideMethod), args: new { layerName = builder.Layer });
            base.BuildAspect(builder);
        }

        [Template]
        public dynamic? OverrideMethod([CompileTime] string? layerName)
        {
            Console.WriteLine("Layer: " + layerName);
            return meta.Proceed();
        }

        public override void Validate(dynamic? value)
        {
            Console.WriteLine("Test");
        }
    }

    // <target>
    public class C
    {
        [DerivedAspect]
        public int Foo { get; set; }
    }
}