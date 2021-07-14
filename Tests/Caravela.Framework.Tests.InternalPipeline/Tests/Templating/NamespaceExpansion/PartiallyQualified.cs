using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace PartiallyQualified
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic? Template()
            {
                var c = new ChildNs.ChildClass();

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

        namespace ChildNs
        {
            class ChildClass
            {
            }
        }
    }
}