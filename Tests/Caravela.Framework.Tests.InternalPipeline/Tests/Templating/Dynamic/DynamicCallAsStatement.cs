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
            meta.Target.Method.Invoke( 1 + 1 ).Foo();
            meta.Target.Method.Invoke( meta.Target.Parameters[0].Value ).Foo();
            meta.Target.Method.Invoke( 1 + 1 );
            meta.Target.Method.Invoke( meta.Target.Parameters[0].Value );

            // Discard assignment
            _ = meta.Target.Method.Invoke( 1 + 1 ).Foo();
            _ = meta.Target.Method.Invoke( meta.Target.Parameters[0].Value ).Foo();
            _ = meta.Target.Method.Invoke( 1 + 1  );
            _ = meta.Target.Method.Invoke( meta.Target.Parameters[0].Value );

            // Local variable assignment
            var x = meta.Target.Method.Invoke( 1 + 1  ).Foo();
            var y = meta.Target.Method.Invoke( meta.Target.Parameters[0].Value ).Foo();
            var a = meta.Target.Method.Invoke( 1 + 1  );
            var b = meta.Target.Method.Invoke( meta.Target.Parameters[0].Value );
            
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