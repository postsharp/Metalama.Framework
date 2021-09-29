using Caravela.Framework.Code;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.PatternMatching.PropertyPattern
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Compile time
            var ct = meta.CompileTime(new object());
            var a1 = ct is IParameter { Index: var index } p && p.DefaultValue.IsNull && index > 0;
            meta.InsertComment("a1 = " + a1 );  
          
            // Run-time
            var a2 = meta.Target.Parameters[0].Value is >= 0 and < 5;
                    
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