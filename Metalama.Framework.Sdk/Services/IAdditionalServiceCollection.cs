// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using Metalama.Framework.Services;
using System;

namespace Metalama.Framework.Engine.Services;

public interface IAdditionalServiceCollection : IGlobalService, IDisposable
{
    void AddService( IProjectService service );
    void AddService( IGlobalService service );
    void AddService( Func<ProjectServiceProvider,IProjectService> service );
    void AddService( Func<GlobalServiceProvider,IGlobalService> service );
    
    
}