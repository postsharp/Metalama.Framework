using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.InternalPipeline.Templating.Syntax.Switch.MetaRunTime
{
      class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x1 = meta.RunTime(0);
            var x2= meta.RunTime(1+1);
            var x3= meta.RunTime(default(int));
            var x4= meta.RunTime(meta.CompileTime(default(int)));
            var x5= meta.RunTime(meta.CompileTime(int.MaxValue));
            
            
            return default;
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