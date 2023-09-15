using Metalama.Framework.Tests.Integration.Tests.Options;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Options.BaseType;

[MyOptions( "Attribute.C" )]
[OptionsAspect]
public class C { }

[OptionsAspect]
public class D : C { }