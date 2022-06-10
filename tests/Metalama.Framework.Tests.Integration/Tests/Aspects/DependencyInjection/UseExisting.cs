using System;
#if TEST_OPTIONS
//@Include(_PullStrategy.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection.UseExisting;

// <target>
[MyAspect]
public class BaseClass
{
    public BaseClass( ICustomFormatter formatter ) { }
}

// <target>
public class DerivedClass : BaseClass
{
    public DerivedClass( ICustomFormatter formatter ) : base( formatter ) { }
}