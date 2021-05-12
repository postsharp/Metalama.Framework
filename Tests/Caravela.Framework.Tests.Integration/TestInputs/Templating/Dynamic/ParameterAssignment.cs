using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;


namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.ParameterAssignment
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var result = meta.Proceed();
            meta.Parameters[0].Value = 5;
            return result;
        }
    }

    [TestOutput]
    class TargetCode
    {
        int Method(out int a)
        {
            a = 1;
            return 1;
        }
    }
}