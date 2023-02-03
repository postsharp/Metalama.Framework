using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Async.ProceedConfigureAwait;

public sealed class TransactionalMethodAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod() => throw new NotSupportedException();

    public override async Task<dynamic?> OverrideAsyncMethod()
    {
        var result = await meta.ProceedAsync().ConfigureAwait(false);
        await meta.This.OnTransactionMethodSuccessAsync();
        return result;
    }
}

// <target>
public class TargetClass
{
    protected async Task OnTransactionMethodSuccessAsync()
    {
        await Task.Yield();
    }

    [TransactionalMethod]
    public async Task<int> DoSomethingAsync()
    {
        await Task.Yield();
        Console.WriteLine("Hello");
        return 42;
    }

    [TransactionalMethod]
    public async Task DoSomethingAsync2()
    {
        await Task.Yield();
        Console.WriteLine("Hello");
    }
}