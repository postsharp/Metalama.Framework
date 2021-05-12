using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;


namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.SerializationError
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Console.WriteLine(meta.RunTime(meta.Method));
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