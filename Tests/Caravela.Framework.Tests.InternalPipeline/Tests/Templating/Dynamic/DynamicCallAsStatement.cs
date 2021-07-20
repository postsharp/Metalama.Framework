// @AllowCompileTimeDynamicCode

using Caravela.Framework.Aspects;
using Caravela.TestFramework;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicCallAsStatement
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Expression statement
            meta.Method.Invoke( 1 + 1 ).Foo();
            meta.Method.Invoke( meta.Parameters[0].Value ).Foo();
            meta.Method.Invoke( 1 + 1 );
            meta.Method.Invoke( meta.Parameters[0].Value );

            // Discard assignment
            _ = meta.Method.Invoke( 1 + 1 ).Foo();
            _ = meta.Method.Invoke( meta.Parameters[0].Value ).Foo();
            _ = meta.Method.Invoke( 1 + 1  );
            _ = meta.Method.Invoke( meta.Parameters[0].Value );

            // Local variable assignment
            var x = meta.Method.Invoke( 1 + 1  ).Foo();
            var y = meta.Method.Invoke( meta.Parameters[0].Value ).Foo();
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