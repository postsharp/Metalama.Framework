using System;
using Castle.DynamicProxy.Generators;
using System.Threading.Tasks;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33404;

public class LoggingAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( $"Executing {meta.Target.Method.ToDisplayString()}" );

        return meta.Proceed();
    }
}

// <target>
public partial class TestClass
{
    [LoggingAspect]
    public async Task<T?> GetAsync<T>( CacheKey key, Func<Task<T>> acquire )
    {
        await Task.Yield();

        return default;
    }

    [LoggingAspect]
    public async Task<T?> GetAsync<T>( CacheKey key, Func<T> acquire )
    {
        await Task.Yield();

        return default;
    }

    [LoggingAspect]
    public T? Get<T>( CacheKey key, Func<Task<T>> acquire )
    {
        return default;
    }

    [LoggingAspect]
    public T? Get<T>( CacheKey key, Func<T> acquire )
    {
        return default;
    }

    [LoggingAspect]
    public IEnumerable<T?> GetEnumerable<T>( CacheKey key, Func<Task<T>> acquire )
    {
        yield return default;
    }

    [LoggingAspect]
    public IEnumerable<T?> GetEnumerable<T>( CacheKey key, Func<T> acquire )
    {
        yield return default;
    }
}