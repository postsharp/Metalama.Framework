using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Layers.Inheritance
{
    [Layers( "First" )]
    public class BaseAspect : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advice.Override( builder.Target, nameof(BaseOverrideMethod), args: new { layerName = builder.Layer } );
        }

        [Template]
        public dynamic? BaseOverrideMethod( [CompileTime] string? layerName )
        {
            Console.WriteLine( "Layer (base): " + layerName );

            return meta.Proceed();
        }
    }

    [Layers("Second")]
    public class DerivedAspect : BaseAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Advice.Override(builder.Target, nameof(DerivedOverrideMethod), args: new { layerName = builder.Layer });

            base.BuildAspect(builder);
        }

        [Template]
        public dynamic? DerivedOverrideMethod([CompileTime] string? layerName)
        {
            Console.WriteLine("Layer (derived): " + layerName);

            return meta.Proceed();
        }
    }

    // <target>
    public class C
    {
        [DerivedAspect]
        public void M() { }
    }
}