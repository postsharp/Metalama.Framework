using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

#pragma warning disable CS0169, CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Dynamic.Cast
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var field = meta.Target.Type.Fields.Single();
            object? clone = null;
            field.Invokers.Base!.SetValue(
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