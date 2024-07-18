#if TEST_OPTIONS
// @BasePathLength(120) This is to trigger a situation where the compile-time assembly name must be trimmed. 
#endif

// Cannot find the compile-time assembly 'dependency_***'.

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug35154;

//<target>
public class TargetClass : BaseClass { }