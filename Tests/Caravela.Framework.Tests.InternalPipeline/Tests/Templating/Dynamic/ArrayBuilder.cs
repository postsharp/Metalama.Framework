using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Caravela.Framework.Shared.Code.ExpressionBuilders;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DynamicArrayBuilder
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var array = new ArrayBuilder();

            foreach (var p in meta.Target.Parameters)
            {
                array.Add( p.Value );
            }

            var array1 = array.ToValue();
            var array2 = array.ToExpression().Value;

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