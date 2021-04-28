using System.Threading;

namespace Caravela.Framework.TestApp.Test
{
    internal class Class1
    {
        [CancelAspect]
#pragma warning disable IDE0060 // Remove unused parameter
        public void Test( CancellationToken cancellationToken )
#pragma warning restore IDE0060 // Remove unused parameter
        {
        }
    }
}
