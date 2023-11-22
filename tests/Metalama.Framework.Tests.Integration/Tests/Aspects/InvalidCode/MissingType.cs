

// Test that using a type that does not exist produces only C# errors, and not confusing Metalama errors.

namespace Metalama.Framework.Tests.Integration.Aspects.InvalidCode.MissingType;

public class C
{
#if TESTRUNNER // Avoid the code to be parsed in the IDE.
    [CompileTime]
    void M(IAspectBuilder<Foo> builder)
    {
        builder.Outbound.SelectMany(t => t);
    }
#endif
}