#if TEST_OPTIONS
//@Include(_PullStrategy.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection.ImplicitConstructor;

// <target>
[MyAspect]
public class BaseClass { }

// <target>
public class DerivedClass : BaseClass { }