using System;
using System.Linq;
using System.Text;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

#pragma warning disable CS0169, CS8618

namespace Caravela.Framework.Tests.Integration.Tests.Templating.Syntax.Array.RunTimeArrayOfCompileTimeSize
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var fields = meta.Type.FieldsAndProperties.Where( f => !f.IsStatic ).ToList();
            var values = new object[fields.Count];

            foreach (int i in meta.CompileTime( Enumerable.Range( 0, fields.Count ) ))
            {
                values[i] = i;
            }
            return default;
            
        }
    }
    
    class TargetCode
    {
        int x;
        public string Y { get; set; }
        
        int Method(int a)
        {
            return a;
        }
    }
}