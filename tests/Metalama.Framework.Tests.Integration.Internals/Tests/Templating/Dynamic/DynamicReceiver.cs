using Metalama.Framework.Aspects;
using Metalama.TestFramework;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DynamicReceiver
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // This
            meta.This.MyMethod();
            meta.This.MyMethod().More();
            meta.This.Value = 5;
            meta.This.MyMethod().More().Value = 5;
            
            // Parameter
            meta.Target.Parameters[0].Value.MyMethod();
            meta.Target.Parameters[0].Value.MyMethod().More();
            
            meta.ThisStatic.Hello();
            
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
        
        public static void Hello() {}
    }
}