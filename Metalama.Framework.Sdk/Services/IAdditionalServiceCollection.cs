// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;
using System;

namespace Metalama.Framework.Engine.Services;

/// <summary>
/// A set of mocks or services injected into the production service providers.
/// </summary>
public interface IAdditionalServiceCollection : IGlobalService, IDisposable
{
    void AddProjectService<T>( T service ) 
        where T : IProjectService;

    void AddGlobalService<T>( T service )
        where T : IGlobalService;

    void AddProjectService<T>( Func<ProjectServiceProvider, T> service ) 
        where T : class, IProjectService;

    void AddGlobalService<T>( Func<GlobalServiceProvider, T> service )
        where T : class, IGlobalService;
}