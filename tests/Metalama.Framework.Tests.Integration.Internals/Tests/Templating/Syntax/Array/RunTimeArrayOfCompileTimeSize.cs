using System;
using System.Linq;
using System.Text;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.Framework;

#pragma warning disable CS0169, CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Syntax.Array.RunTimeArrayOfCompileTimeSize
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var fields = meta.Target.Type.FieldsAndProperties.Where( f => !f.IsStatic & !f.IsImplicitlyDeclared ).ToList();
            var values = meta.RunTime( new object[fields.Count] );

            foreach (var i in meta.CompileTime( Enumerable.Range( 0, fields.Count ) ))
            {
                values[i] = i;
            }

            return default;
        }
    }

    internal class TargetCode
    {
        private int x;

        public string Y { get; set; }

        private int Method( int a )
        {
            return a;
        }
    }
}