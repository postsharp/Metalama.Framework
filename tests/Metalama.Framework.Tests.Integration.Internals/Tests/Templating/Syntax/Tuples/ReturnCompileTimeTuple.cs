using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Syntax.Tuples.ReturnCompileTimeTuple
{
    class Aspect
    {
        [TestTemplate]
        (int,Type) Template()
        {
            return ( meta.Target.Method.Parameters.Count, typeof(int) );
        }
    }

    class TargetCode
    {
        (int,Type) Method(int a)
        {
            return meta.CompileTime( ( a, typeof(int) ) );
        }
    }
}