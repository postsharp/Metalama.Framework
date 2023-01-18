using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;


namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.SerializationError
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Console.WriteLine(meta.RunTime(meta.Target.Method));
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