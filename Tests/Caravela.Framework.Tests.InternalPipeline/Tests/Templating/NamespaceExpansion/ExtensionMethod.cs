using System.Collections.Generic;
using System.Linq;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace ExtensionMethod
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic? Template()
            {
                var list = new List<int>();

                // No argument.
                var max = list.Max();

                // Constant argument.
                var take = list.Take(1);

                // Dynamic argument.
                var take2 = list.Take((int)meta.Parameters[0].Value);

                return meta.Proceed();
            }
        }

        class TargetCode
        {
            int Method(int a)
            {
                return a;
            }
        }
    }
}