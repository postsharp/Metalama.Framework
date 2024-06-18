using System.Threading.Tasks;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32900;

public sealed class TestAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var result = meta.Target.Method.GetAsyncInfo().ResultType.DefaultValue();

        try
        {
            result = meta.Proceed();
        }
        catch { }

        return result;
    }
}

// <target>
public partial class TargetClass
{
    [TestAspect]
    public async Task AsyncTaskMethod()
    {
        var result = 42;
        await Task.Yield();
        _ = result;
    }

    [TestAspect]
    public async Task<int> AsyncTaskIntMethod()
    {
        var result = 42;
        await Task.Yield();

        return result;
    }
}