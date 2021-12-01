using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.SyntaxBuilders;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.InterpolatedStringBuilderEscape
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
        
            // Normal literals.
            Console.WriteLine("\\\n{}\"");
            Console.WriteLine( meta.CompileTime( "\\" + "\n{}\"" ));
            
        
            // Interpolated string.
            var s = new InterpolatedStringBuilder();
            s.AddText( "{ " );
            s.AddText( "$" );
            s.AddText( "\\" );
            s.AddText( "\n" );
            s.AddText(" }");
            
            var a = s.ToValue();
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