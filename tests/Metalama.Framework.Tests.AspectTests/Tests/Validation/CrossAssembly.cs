using System;
using System.Collections.Generic;

#pragma warning disable CS0168, CS8618, CS0169

namespace Metalama.Framework.Tests.AspectTests.Validation.CrossAssembly
{
    // Base type.
    internal class DerivedClass : ValidatedClass
    {
        // Field type.
        private ValidatedClass _field1;

        // Typeof in field initializer.
        private Type _field2 = typeof(ValidatedClass);

        private ValidatedClass? Method( ValidatedClass[] param1, List<ValidatedClass> param2 )
        {
            return null;
        }
    }

    internal class ReferencingClass
    {
        private void Method()
        {
            ValidatedClass variable;
            ValidatedClass.Method( typeof(ValidatedClass) );
        }
    }
}