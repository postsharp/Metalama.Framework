// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.DesignTime.AspectExplorer;
using Metalama.Framework.DesignTime.Contracts.AspectExplorer;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio.AspectExplorer;

internal sealed class AspectDatabase : IAspectDatabaseService2, IDisposable
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
            typeIdResolver.TryResolveId( new SerializableTypeId( serializableTypeId ), out var symbol );

            return (INamedTypeSymbol?) symbol;
        }

        // Resolving can fail when the pipeline is paused and the aspect class does not exist in the modified compilation.
        return aspectClasses.Select( ResolveOrNull ).WhereNotNull().ToArray();
    }

    private static AspectExplorerAspectInstance ToVersion1( AspectExplorerAspectInstance2 aspectInstance )
        => new()
        {
            TargetDeclaration = aspectInstance.TargetDeclaration,
            TargetDeclarationKind = aspectInstance.TargetDeclarationKind,
            Transformations = aspectInstance.Transformations.Select( ToVersion1 ).ToArray()
        };

    private static AspectExplorerAspectTransformation ToVersion1( AspectExplorerAspectTransformation2 transformation )
        => new()
        {
            TargetDeclaration = transformation.TargetDeclaration,
            TargetDeclarationKind = transformation.TargetDeclarationKind,
            Description = transformation.Description
        };

    public async Task GetAspectInstancesAsync(
        Compilation compilation,
        INamedTypeSymbol aspectClass,
        IEnumerable<AspectExplorerAspectInstance>[] result,
        CancellationToken cancellationToken )
    {
        var version2Result = new IEnumerable<AspectExplorerAspectInstance2>[1];

        await this.GetAspectInstancesAsync( compilation, aspectClass, version2Result, cancellationToken );

        result[0] = version2Result[0].Select( ToVersion1 ).ToArray();
    }

    public async Task GetAspectInstancesAsync(
        Compilation compilation,
        INamedTypeSymbol aspectClass,
        IEnumerable<AspectExplorerAspectInstance2>[] result,
        CancellationToken cancellationToken )
    {
        var projectKey = compilation.GetProjectKey();

        if ( !projectKey.IsMetalamaEnabled )
        {
            result[0] = [];

            return;
        }

        var analysisProcessApi = await this._userProcessEndpoint.GetApiAsync( projectKey, nameof(this.GetAspectInstancesAsync), cancellationToken );

        var aspectInstances = await analysisProcessApi.GetAspectInstancesAsync( projectKey, aspectClass.ContainingAssembly.Name, aspectClass.GetSerializableTypeId().Id, cancellationToken );

        result[0] = GetAspectInstances().ToArray();

        IEnumerable<AspectExplorerAspectInstance2> GetAspectInstances()
        {
            foreach ( var aspectInstance in aspectInstances )
            {
                var targetDeclaration = ResolveToSymbol( aspectInstance.TargetDeclarationId, out var targetDeclarationKind );

                if ( targetDeclaration is null )
                {
                    continue;
                }

                yield return new()
                {
                    TargetDeclaration = targetDeclaration,
                    TargetDeclarationKind = targetDeclarationKind,
                    Transformations = GetTransformations( aspectInstance ).ToArray()
                };
            }
        }

        IEnumerable<AspectExplorerAspectTransformation2> GetTransformations( AspectDatabaseAspectInstance aspectInstance )
        {
            foreach ( var transformation in aspectInstance.Transformations )
            {
                var targetDeclaration = ResolveToSymbol( transformation.TargetDeclarationId, out var targetDeclarationKind );

                if ( targetDeclaration is null )
                {
                    continue;
                }

                var transformedDeclaration = ResolveToSymbol( transformation.TransformedDeclarationId, out var transformedDeclarationKind );
                Invariant.Assert( transformedDeclarationKind == default );

                yield return new()
                {
                    TargetDeclaration = targetDeclaration,
                    TargetDeclarationKind = targetDeclarationKind,
                    Description = transformation.Description,
                    TransformedDeclaration = transformedDeclaration,
                    FilePath = transformation.FilePath
                };
            }
        }

        ISymbol? ResolveToSymbol( string? id, out AspectExplorerDeclarationKind kind )
        {
            if ( id is null )
            {
                kind = AspectExplorerDeclarationKind.Default;

                return null;
            }

            var symbol = new SerializableDeclarationId( id ).ResolveToSymbolOrNull( compilation, out var isReturnParameter );

            kind = isReturnParameter ? AspectExplorerDeclarationKind.ReturnParameter : AspectExplorerDeclarationKind.Default;

            return symbol;
        }
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