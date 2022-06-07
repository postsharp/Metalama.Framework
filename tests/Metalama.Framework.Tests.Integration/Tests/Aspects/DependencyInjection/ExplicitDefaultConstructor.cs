#if TEST_OPTIONS
//@Include(_PullStrategy.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection.ExplicitDefaultConstructor;

// <target>
[MyAspect]
public class BaseClass
{
    public BaseClass() { }
}

// <target>
public class DerivedClass : BaseClass
{
    public DerivedClass() : base() { }
}