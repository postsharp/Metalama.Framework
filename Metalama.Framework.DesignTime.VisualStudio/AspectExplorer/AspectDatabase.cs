// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.DesignTime.AspectExplorer;
using Metalama.Framework.DesignTime.Contracts.AspectExplorer;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio.AspectExplorer;

internal sealed class AspectDatabase : IAspectDatabaseService, IDisposable
{
    private readonly UserProcessServiceHubEndpoint _userProcessEndpoint;

    public AspectDatabase( GlobalServiceProvider serviceProvider )
    {
        this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();

        this._userProcessEndpoint.AspectClassesChanged += this.OnAspectClassesChanged;
        this._userProcessEndpoint.AspectInstancesChanged += this.OnAspectInstancesChanged;
    }

    public async Task<IEnumerable<INamedTypeSymbol>> GetAspectClassesAsync( Compilation compilation, CancellationToken cancellationToken )
    {
        var projectKey = compilation.GetProjectKey();

        if ( !projectKey.IsMetalamaEnabled )
        {
            return [];
        }

        var analysisProcessApi = await this._userProcessEndpoint.GetApiAsync( projectKey, nameof(this.GetAspectClassesAsync), cancellationToken );

        var aspectClasses = await analysisProcessApi.GetAspectClassesAsync( projectKey, cancellationToken );

        var typeIdResolver = CompilationContextFactory.GetInstance( compilation ).SerializableTypeIdResolver;

        INamedTypeSymbol? ResolveOrNull( string serializableTypeId )
        {
            try
            {
                return (INamedTypeSymbol) typeIdResolver.ResolveId( new( serializableTypeId ) );
            }
            catch ( InvalidOperationException )
            {
                return null;
            }
        }

        // Resolving can fail when the pipeline is paused and the aspect class does not exist in the modified compilation.
        return aspectClasses.Select( ResolveOrNull ).WhereNotNull().ToArray();
    }

    public async Task GetAspectInstancesAsync(
        Compilation compilation,
        INamedTypeSymbol aspectClass,
        IEnumerable<AspectExplorerAspectInstance>[] result,
        CancellationToken cancellationToken )
    {
        var projectKey = compilation.GetProjectKey();

        if ( !projectKey.IsMetalamaEnabled )
        {
            result[0] = [];

            return;
        }

        var analysisProcessApi = await this._userProcessEndpoint.GetApiAsync( projectKey, nameof(this.GetAspectInstancesAsync), cancellationToken );

        var aspectInstances = await analysisProcessApi.GetAspectInstancesAsync( projectKey, aspectClass.GetSerializableTypeId().Id, cancellationToken );

        result[0] = GetAspectInstances().ToArray();

        IEnumerable<AspectExplorerAspectInstance> GetAspectInstances()
        {
            foreach ( var aspectInstance in aspectInstances )
            {
                var targetDeclaration = ResolveToSymbol( aspectInstance.TargetDeclarationId );

                if ( targetDeclaration is null )
                {
                    continue;
                }

                yield return new AspectExplorerAspectInstance
                {
                    TargetDeclaration = targetDeclaration,
                    Transformations = GetTransformations( aspectInstance ).ToArray()
                };
            }
        }

        IEnumerable<AspectExplorerAspectTransformation> GetTransformations( AspectDatabaseAspectInstance aspectInstance )
        {
            foreach ( var transformation in aspectInstance.Transformations )
            {
                var targetDeclaration = ResolveToSymbol( transformation.TargetDeclarationId );

                if ( targetDeclaration is null )
                {
                    continue;
                }

                yield return new AspectExplorerAspectTransformation
                {
                    TargetDeclaration = targetDeclaration,
                    Description = transformation.Description
                };
            }
        }

        ISymbol? ResolveToSymbol( string? id ) => id is null ? null : new SerializableDeclarationId( id ).ResolveToSymbolOrNull( compilation );
    }

    public event Action<string>? AspectClassesChanged;

    private void OnAspectClassesChanged( ProjectKey projectKey ) => this.AspectClassesChanged?.Invoke( projectKey.AssemblyName );

    public event Action<string>? AspectInstancesChanged;

    private void OnAspectInstancesChanged( ProjectKey projectKey ) => this.AspectInstancesChanged?.Invoke( projectKey.AssemblyName );

    public void Dispose()
    {
        this._userProcessEndpoint.AspectClassesChanged -= this.OnAspectClassesChanged;
        this._userProcessEndpoint.AspectInstancesChanged -= this.OnAspectInstancesChanged;
    }
}