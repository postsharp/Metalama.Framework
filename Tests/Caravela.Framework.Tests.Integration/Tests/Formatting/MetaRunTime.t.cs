using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Tests.Formatting.MetaRunTime
{
   
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
dynamic Template() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
}