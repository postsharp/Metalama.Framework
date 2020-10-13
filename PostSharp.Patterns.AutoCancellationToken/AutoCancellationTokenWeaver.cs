using System;
using Microsoft.CodeAnalysis;
using PostSharp.Framework.Sdk;

namespace PostSharp.Patterns.AutoCancellationToken
{
    [AspectWeaver(typeof(AutoCancellationTokenAttribute))]
    class AutoCancellationTokenWeaver : IAspectWeaver
    {
        public Compilation Transform(AspectWeaverContext context)
        {
            throw new NotImplementedException();
        }
    }
}
