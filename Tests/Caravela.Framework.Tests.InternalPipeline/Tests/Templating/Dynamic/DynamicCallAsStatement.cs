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
            meta.Method.Invoke( 1 + 1  ).AssertNotNull();
            meta.Method.Invoke( meta.Parameters[0].Value ).AssertNotNull();
            meta.Method.Invoke( 1 + 1  );
            meta.Method.Invoke( meta.Parameters[0].Value );
            
            // Discard assignment
            _ = meta.Method.Invoke( 1 + 1 ).AssertNotNull();
            _ = meta.Method.Invoke( meta.Parameters[0].Value ).AssertNotNull();
            _ = meta.Method.Invoke( 1 + 1  );
            _ = meta.Method.Invoke( meta.Parameters[0].Value );

            // Local variable assignment
            var x = meta.Method.Invoke( 1 + 1  ).AssertNotNull();
            var y = meta.Method.Invoke( meta.Parameters[0].Value ).AssertNotNull();
            var a = meta.Method.Invoke( 1 + 1  );
            var b = meta.Method.Invoke( meta.Parameters[0].Value );
            
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