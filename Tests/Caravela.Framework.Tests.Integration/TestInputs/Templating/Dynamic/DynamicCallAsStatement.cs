using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;


namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicCallAsStatement
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Expression statement
            meta.Method.Invoke( meta.This, meta.Parameters[0].Value ).AssertNotNull();
            
            // Assignment
            _ = meta.Method.Invoke( meta.This, 1 );
            
            return default;
        }
    }

    [TestOutput]
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}