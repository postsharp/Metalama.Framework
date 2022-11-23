// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Testing;

public class TestServiceFactory : IDisposable
{
    private readonly ConcurrentStack<IDisposable> _disposables = new();

    public TestServiceFactory()
    {
        this.GlobalServices.ServiceCreated += this.OnServiceCreated;
        this.ProjectServices.ServiceCreated += this.OnServiceCreated;
    }

    private void OnServiceCreated( object obj )
    {
        if ( obj is IDisposable disposable )
        {
            this._disposables.Push( disposable );
        }
    }

    public TestServiceFactory( params object[] mocks ) : this()
    {
        foreach ( var mock in mocks )
        {
            if ( mock is IProjectService projectService )
            {
                this.ProjectServices.Add( mock.GetType(), _ => projectService );
            }
            else if ( mock is IService globalService )
            {
                this.GlobalServices.Add( mock.GetType(), _ => globalService );
            }
            else
            {
                throw new ArgumentException( $"The object '{mock}' is not a valid service." );
            }
        }
    }

    public ServiceFactory<IService> GlobalServices { get; } = new();

    public ServiceFactory<IProjectService> ProjectServices { get; } = new();

    public ServiceFactory<IBackstageService> BackstageServices { get; } = new();

    public void Dispose()
    {
        while ( this._disposables.TryPop( out var disposable ) )
        {
            disposable.Dispose();
        }
    }
}