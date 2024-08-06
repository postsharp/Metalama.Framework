#if TEST_OPTIONS
// @DesignTime
# endif

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.IntegrationTests.Aspects.InvalidCode.TransitiveValidator_SerializableId;

public class TestClass
{
    public void Foo(DerivedReferencedClass x)
    {
        _ = x.Foo;
    }
}