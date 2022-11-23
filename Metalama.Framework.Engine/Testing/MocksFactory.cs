// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Testing;

// This object is a service itself. The test runner registers it as a global service because some pipelines
// recreate the service providers from the global provider.
public class MocksFactory : IDisposable, IGlobalService
{
    private readonly ConcurrentStack<IDisposable> _disposables = new();

    public MocksFactory() { }

    public MocksFactory( params object[] mocks ) : this()
    {
        foreach ( var mock in mocks )
        {
            switch (mock)
            {
                case IProjectService projectService:
                    this.ProjectServices.Add( projectService );

                    break;

                case IGlobalService globalService:
                    this.GlobalServices.Add( globalService );

                    break;

                case IBackstageService backstageService:
                    this.BackstageServices.Add( backstageService );

                    break;

                default:
                    throw new ArgumentException( $"The object '{mock}' is not a valid service." );
            }

            if ( mock is IDisposable disposable )
            {
                this._disposables.Push( disposable );
            }
        }
    }

    public ServiceProviderBuilder<IGlobalService> GlobalServices { get; } = new();

    public ServiceProviderBuilder<IProjectService> ProjectServices { get; } = new();

    public ServiceProviderBuilder<IBackstageService> BackstageServices { get; } = new();

    public void Dispose()
    {
        while ( this._disposables.TryPop( out var disposable ) )
        {
            disposable.Dispose();
        }
    }
}