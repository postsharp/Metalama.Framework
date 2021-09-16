using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code.Syntax;
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

            var array1 = array.ToArray();
            var array2 = ( (IExpressionBuilder)array ).ToExpression().Value;

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