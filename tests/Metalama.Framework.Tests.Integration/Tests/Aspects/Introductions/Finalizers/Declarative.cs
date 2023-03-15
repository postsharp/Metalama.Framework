using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Finalizers.Declarative;

/* 
 * Tests that using the declarative finalizer introduction produces an error.
 */

public class TestAttribute : TypeAspect
{
    [Introduce]
    ~TestAttribute() { }
}

// <target>
[Test]
internal class Target
{
}