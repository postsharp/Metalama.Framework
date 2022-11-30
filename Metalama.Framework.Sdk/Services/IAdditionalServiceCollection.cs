// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using Metalama.Framework.Services;
using System;

namespace Metalama.Framework.Engine.Services;

public interface IAdditionalServiceCollection : IGlobalService, IDisposable
{
    void AddProjectService<T>( T service ) where T : IProjectService;

    void AddGlobalService<T>( T service ) where T : IGlobalService;

    void AddProjectService<T>( Func<ProjectServiceProvider, T> service ) where T : class, IProjectService;

    void AddGlobalService<T>( Func<GlobalServiceProvider, T> service ) where T : class, IGlobalService;


}