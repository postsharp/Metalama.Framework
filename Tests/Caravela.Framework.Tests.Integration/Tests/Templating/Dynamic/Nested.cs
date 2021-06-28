using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Tests.Templating.Dynamic.Cast
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var field = meta.NamedType.Fields.Single();
            object clone = null;
            field.Invokers.Base.SetValue(
                clone, 
                meta.Cast(field.Type, ((ICloneable)field.Invokers.Base.GetValue(meta.This)).Clone()));
            
            return default;
        }
    }

    // <target>
    class TargetCode : IDisposable
    {
        private int field;
        
        int Method(int a)
        {
            return a;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}