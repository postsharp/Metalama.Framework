using System;
using System.Linq;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Bugs.Bug30599
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Advice.Override(builder.Target, nameof(Template));
        }

        [Template]
        public dynamic? Template()
        {
            var disposedField = meta.Target.Method.DeclaringType.Fields.OfName("_disposed").FirstOrDefault();

            if ((bool)disposedField!.Value)
            {
                throw new InvalidOperationException("The object has already been disposed");
            }

            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetCode
    {
        public bool _disposed;

        [Aspect]
        private async Task<int> Method( int a )
        {
            await Task.Yield();
            return a;
        }
    }
}