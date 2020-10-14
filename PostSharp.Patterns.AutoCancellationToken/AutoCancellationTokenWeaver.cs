using System;
using Microsoft.CodeAnalysis.CSharp;
using PostSharp.Framework.Sdk;

namespace PostSharp.Patterns.AutoCancellationToken
{
    [AspectWeaver(typeof(AutoCancellationTokenAttribute))]
    class AutoCancellationTokenWeaver : IAspectWeaver
    {
        public CSharpCompilation Transform(AspectWeaverContext context)
        {
            throw new NotImplementedException();
        }
    }
}
