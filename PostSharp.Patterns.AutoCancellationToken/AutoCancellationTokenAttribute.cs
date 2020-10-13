using System;
using PostSharp.Framework.Aspects;

namespace PostSharp.Patterns.AutoCancellationToken
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]

    public class AutoCancellationTokenAttribute : Attribute, IAspect
    {
        public void Initialize(IAspectBuilder aspectBuilder) => throw new NotSupportedException();
    }
}
