using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.Bug28768
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var parameterNamesTypes=  meta.RunTime( meta.Parameters.Select(p => p.ParameterType.ToType()).ToArray() );
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