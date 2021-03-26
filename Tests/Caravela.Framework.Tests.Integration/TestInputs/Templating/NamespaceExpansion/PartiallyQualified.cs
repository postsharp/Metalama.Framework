using System;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace PartiallyQualified
    {
        class Aspect
        {
            [TestTemplate]
            dynamic Template()
            {
                var c = new ChildNs.ChildClass();
                
                return proceed();
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