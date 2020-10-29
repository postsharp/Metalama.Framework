using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Patterns.AutoCancellationToken
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]

    public class AutoCancellationTokenAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder) => throw new NotSupportedException();
    }
}
