using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace Qualified
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic? Template()
            {
               System.Console.WriteLine("Oops");
                
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