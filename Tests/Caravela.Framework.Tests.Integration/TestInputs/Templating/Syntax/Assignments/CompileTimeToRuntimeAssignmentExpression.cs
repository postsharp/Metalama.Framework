#pragma warning disable CS0219

using System;
using System.Text;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.CompileTimeToRuntimeAssignmentExpression
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime(0);
            var y = 0;
            
            x = y = 1;
            
            
            return null;
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