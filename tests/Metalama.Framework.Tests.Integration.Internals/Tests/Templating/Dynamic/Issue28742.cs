using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

#pragma warning disable CS0169, CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Dynamic.Issue28742
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            foreach (var fieldOrProperty in meta.Target.Type.FieldsAndProperties)
            {
                if (!fieldOrProperty.IsImplicitlyDeclared && fieldOrProperty.IsAutoPropertyOrField.GetValueOrDefault())
                {
                    var value = fieldOrProperty.Invokers.Final.GetValue( fieldOrProperty.IsStatic ? null : meta.This );
                    Console.WriteLine( $"{fieldOrProperty.Name}={value}" );
                }
            }

            return default;
        }
    }

    // Placeholder implementation of a cache because the hosted try.postsharp.net does not allow for MemoryCache.

    // <target>
    internal class TargetCode
    {
        private int a;

        public string B { get; set; }

        private static int c;

        private void Method() { }
    }
}