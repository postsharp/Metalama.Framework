using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Caravela.Framework.TestApp.Test
{
    class Class1
    {
        [CancelAspect]
        public void Test(CancellationToken cancellationToken)
        {

        }
    }
}
