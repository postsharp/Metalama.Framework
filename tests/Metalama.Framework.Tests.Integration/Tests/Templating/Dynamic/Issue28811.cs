using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

#pragma warning disable CS0169, CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Dynamic.Issue28811
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var field = meta.Target.Type.FieldsAndProperties.Single();

            var clone = meta.This;
            var clonedValue = meta.This;
            field.With( (IExpression)clone ).Value = clonedValue;

            return default;
        }
    }

    // Placeholder implementation of a cache because the hosted try.postsharp.net does not allow for MemoryCache.

    // <target>
    internal class TargetCode
    {
        private int a;

        private void Method() { }
    }
}