using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#if TEST_OPTIONS
// @MainMethod(TestMain)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.TestFramework.ReferenceDependencyFromMain;

class Program
{
    static void TestMain() => Dependency.M();
}