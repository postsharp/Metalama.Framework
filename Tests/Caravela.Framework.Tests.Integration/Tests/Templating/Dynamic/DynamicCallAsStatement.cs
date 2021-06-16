using Caravela.Framework.Aspects;
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
            meta.Method.Invoke( meta.Parameters[0].Value ).AssertNotNull();
            
            // Assignment
            _ = meta.Method.Invoke( 1 );
            
            return default;
        }
    }

    // <target>
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}