using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Layers.Inheritance_CrossAssembly
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
}