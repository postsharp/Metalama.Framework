using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using PostSharp.Framework.Sdk;

namespace PostSharp.Patterns.AutoCancellationToken
{
    class AutoCancellationTokenWeaver : IAspectWeaver
    {
        public Compilation Transform(AspectWeaverContext context)
        {
            throw new NotImplementedException();
        }
    }
}
