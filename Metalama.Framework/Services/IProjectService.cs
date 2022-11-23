using Metalama.Framework.Aspects;

namespace Metalama.Framework.Services;

/// <summary>
/// Base interface to be inherited by all classes and interfaces that implement project-scoped services.
/// </summary>
/// <seealso cref="IProjectService"/>
[CompileTime]
public interface IProjectService : IService { }