using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Syntax;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicArrayBuilder
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var array = new ArrayBuilder();
            
            foreach ( var p in meta.Target.Parameters )
            {
                array.Add( p.Value );
            }
            
            var a = array.ToArray();
            return default;
        }
    }

    // <target>
    class TargetCode
    {
        int Method(int a, string c, DateTime dt)
        {
            return a;
        }
    }
}