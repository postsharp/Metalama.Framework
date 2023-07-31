using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.TemplateReturnType_Errors;

public class VoidAttribute : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(Template));
    }

    [Template]
    public void Template()
    {
        Console.WriteLine("void");
        meta.Proceed();
    }
}

public class DynamicAttribute : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(Template));
    }

    [Template]
    public dynamic? Template()
    {
        Console.WriteLine("dynamic");
        return meta.Proceed();
    }
}

public class TaskAttribute : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(Template));
    }

    [Template]
    public async Task Template()
    {
        Console.WriteLine("Task");
        await meta.ProceedAsync();
    }
}

public class TaskDynamicAttribute : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(Template));
    }

    [Template]
    public async Task<dynamic?> Template()
    {
        Console.WriteLine("dynamic");
        return await meta.ProceedAsync();
    }
}

// <target>
internal class TargetClass
{
    [Task]
    [TaskDynamic]
    public void SyncVoid()
    {
        Console.WriteLine("This is the original method.");
    }

    public async void AsyncVoid()
    {
        await Task.Yield();
        Console.WriteLine("This is the original method.");
    }

    [Void]
    [Task]
    [TaskDynamic]
    public int Int()
    {
        Console.WriteLine("This is the original method.");
        return 42;
    }

    [Void]
    public Task SyncTask()
    {
        Console.WriteLine("This is the original method.");
        return Task.CompletedTask;
    }

    [Void]
    public async Task AsyncTask()
    {
        await Task.Yield();
        Console.WriteLine("This is the original method.");
    }

    [Void]
    [Task]
    public Task<int> SyncTaskInt()
    {
        Console.WriteLine("This is the original method.");
        return Task.FromResult(42);
    }

    [Void]
    [Task]
    public async Task<int> AsyncTaskInt()
    {
        await Task.Yield();
        Console.WriteLine("This is the original method.");
        return 42;
    }

    [Void]
    public ValueTask SyncValueTask()
    {
        Console.WriteLine("This is the original method.");
        return new();
    }

    [Void]
    public async ValueTask AsyncValueTask()
    {
        await Task.Yield();
        Console.WriteLine("This is the original method.");
    }

    [Void]
    [Task]
    public ValueTask<int> SyncValueTaskInt()
    {
        Console.WriteLine("This is the original method.");
        return new(42);
    }

    [Void]
    [Task]
    public async ValueTask<int> AsyncValueTaskInt()
    {
        await Task.Yield();
        Console.WriteLine("This is the original method.");
        return 42;
    }
}
