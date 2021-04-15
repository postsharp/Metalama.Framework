using Caravela.Framework.Project;

using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

#pragma warning disable CS8632 // Cannot convert null literal to non-nullable reference type.

namespace Caravela.Framework.Tests.Integration.TestInputs.MagicKeywords.GenericCompileTimeWithRunTimeTypeArg
{
    namespace UsingStatic
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic Template()
            {
                var x = compileTime<TargetCode?>(null);
                var y = compileTime<TargetCode>(null);

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
    }
}