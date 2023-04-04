#if TEST_OPTIONS
// @FormatCompileTimeCode(false)
#endif

using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LocalFunctions;

public class RetryAttribute : OverrideMethodAspect
{
    // Template for non-async methods.
    public override dynamic? OverrideMethod()
    {
        return meta.Proceed();
    }

    // Template for async methods.
    public override async Task<dynamic?> OverrideAsyncMethod()
    {
        async Task<object?> ExecuteCoreAsync()
        {
            var result = await meta.ProceedAsync();

            return result;
        }

        return await Task.Run( ExecuteCoreAsync );
    }
}

// <target>
internal class C
{
    [Retry]
    private void Foo() { }

    [Retry]
    private async Task FooAsync() { }
}