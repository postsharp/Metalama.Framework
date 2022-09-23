#if TEST_OPTIONS
// @RequireOrderedAspects
#endif

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Order.UnorderedError;

public class Aspect1 : TypeAspect { }

public class Aspect2 : TypeAspect { }

[Aspect1]
[Aspect2]
public class TargetClass { }