using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;


namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.SerializationErrorWeaklyTyped
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            object method = meta.Target.Method;
            Console.WriteLine(meta.RunTime(method));
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