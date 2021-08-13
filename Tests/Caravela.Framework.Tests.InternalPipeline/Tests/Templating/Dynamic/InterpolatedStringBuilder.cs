using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Syntax;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.InterpolatedStringBuilderT
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var s = InterpolatedStringBuilder.Create();
            s.AddText( meta.Target.Method.Name + "(" );
            
            foreach ( var p in meta.Target.Parameters )
            {
                if ( p.Index > 0 )
                {
                    s.AddText(", ");
                }
                s.AddText( $"{p.Name}=" );
                s.AddExpression(p.Value);
            }
            s.AddText(")");
            
            var a = s.ToInterpolatedString();
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