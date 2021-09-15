using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Caravela.Framework.Code.ExpressionBuilders;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.InterpolatedStringBuilderT
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var s = new InterpolatedStringBuilder();
            s.AddText( meta.Target.Method.Name + "(" );

            foreach (var p in meta.Target.Parameters)
            {
                if (p.Index > 0)
                {
                    s.AddText( ", " );
                }

                s.AddText( $"{p.Name}=" );
                s.AddExpression( p.Value );
            }

            s.AddText( ")" );

            var is1 = s.ToValue();
            var is2 = s.ToExpression().Value;

            return default;
        }
    }

    // <target>
    internal class TargetCode
    {
        private int Method( int a, string c, DateTime dt )
        {
            return a;
        }
    }
}
