using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Syntax.Tuples.ReturnCompileTimeTuple
{
    class Aspect
    {
        [TestTemplate]
        (int,Type) Template()
        {
            var t =  ( meta.Target.Method.Parameters.Count, typeof(int) );

            return (t.Count + 1, t.Item2);

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