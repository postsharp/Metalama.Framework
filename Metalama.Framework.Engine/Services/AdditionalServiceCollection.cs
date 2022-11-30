// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Framework.Services;
using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Services;

/// <summary>
/// A set of mocks or services injected into the production service providers.
/// </summary>
/// <remarks>
/// This object is a service itself. The test runner registers it as a global service because some pipelines
/// recreate the service providers from the global provider.
/// </remarks>
public class AdditionalServiceCollection : IAdditionalServiceCollection
{
    private readonly ConcurrentStack<IDisposable> _disposables = new();

    public AdditionalServiceCollection() { }

    public AdditionalServiceCollection( params object[] additionalServices ) : this()
    {
        foreach ( var service in additionalServices )
        {
            switch ( service )
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
                    throw new ArgumentException( $"The object '{service}' is not a valid service." );
            }

            if ( service is IDisposable disposable )
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

    public void AddService( IProjectService service ) => this.ProjectServices.Add( service );

    public void AddService( IGlobalService service ) => this.GlobalServices.Add( service );

    public void AddService( Func<ProjectServiceProvider, IProjectService> service ) => this.ProjectServices.Add( provider => service( provider ) );

    public void AddService( Func<GlobalServiceProvider, IGlobalService> service ) => this.GlobalServices.Add( provider => service( provider ) );
}