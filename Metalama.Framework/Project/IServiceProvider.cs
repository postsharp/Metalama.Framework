using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Project;

/// <summary>
/// A strongly-typed variant of <see cref="IServiceProvider"/> that returns services for a given scope.
/// </summary>
/// <typeparam name="TBase">The base interface for the services in the scope.</typeparam>
[CompileTime]
public interface IServiceProvider<TBase> : IServiceProvider
{
    T? GetService<T>() where T : class, TBase;
}