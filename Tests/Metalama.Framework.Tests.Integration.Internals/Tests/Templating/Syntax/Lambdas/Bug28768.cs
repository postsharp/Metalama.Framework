using System;
using System.Linq;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lambdas.Bug28768
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var parameterNamesTypes=  meta.RunTime( meta.Target.Parameters.Select(p => ( (IParameter)p ).Type.ToType()).ToArray() );
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        int Method(int a, string b)
        {
            return a;
        }
    }
}