#if TEST_OPTIONS
// @Include(Imported\Imported.cs)
#endif
#pragma warning disable 169

using Metalama.Framework.Tests.Integration.Tests.TestFramework.Imported;

namespace Metalama.Framework.Tests.Integration.Tests.TestFramework
{
    [ImportedAspect]
    public class IncludeDirective { }
}