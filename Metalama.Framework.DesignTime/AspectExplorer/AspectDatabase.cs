// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Introspection;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.AspectExplorer;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable: WeakCache does not need to be disposed
public sealed class AspectDatabase : IGlobalService, IRpcApi
#pragma warning restore CA1001
{
    private readonly ILogger _logger;
    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;
    private readonly WorkspaceProvider _workspaceProvider;
    private readonly AnalysisProcessEventHub _eventHub;

    private readonly WeakCache<Compilation, ImmutableArray<IIntrospectionAspectInstance>> _aspectInstanceCache = new();

    internal AspectDatabase( GlobalServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "AspectDatabase" );
        this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
        this._workspaceProvider = serviceProvider.GetRequiredService<WorkspaceProvider>();
        this._eventHub = serviceProvider.GetRequiredService<AnalysisProcessEventHub>();
    }

    private async Task<(DesignTimeAspectPipeline Pipeline, Compilation Compilation)?> GetPipelineAndCompilationAsync(
        ProjectKey projectKey,
        CancellationToken cancellationToken )
    {
        var project = await this._workspaceProvider.GetProjectAsync( projectKey, cancellationToken );

        if ( project == null )
        {
            this._logger.Warning?.Log( $"GetPipelineAndCompilationAsync('{projectKey}'): cannot get the project '{projectKey}'." );

            return null;
        }

        var pipeline = this._pipelineFactory.GetOrCreatePipeline( project, cancellationToken.ToTestable() );

        if ( pipeline == null )
        {
            this._logger.Warning?.Log( $"GetPipelineAndCompilationAsync('{projectKey}'): cannot get the pipeline for project '{projectKey}'." );

            return null;
        }

        var compilation = await project.GetCompilationAsync( cancellationToken );

        if ( compilation == null )
        {
            this._logger.Warning?.Log( $"GetPipelineAndCompilationAsync('{projectKey}'): cannot get the compilation for project '{projectKey}'." );

            return null;
        }

        return (pipeline, compilation);
    }

    public async Task<IEnumerable<string>> GetAspectClassesAsync( ProjectKey projectKey, CancellationToken cancellationToken )
    {
        if ( await this.GetPipelineAndCompilationAsync( projectKey, cancellationToken ) is not var (pipeline, compilation) )
        {
            return [];
        }

        await pipeline.GetConfigurationAsync(
            PartialCompilation.CreateComplete( compilation ),
            false,
            AsyncExecutionContext.Get(),
            cancellationToken.ToTestable() );

        var aspectClasses = pipeline.AspectClasses ?? [];

        var aspectClassesIds = aspectClasses
            .OfType<AspectClass>()
            .Where( aspectClass => !aspectClass.IsAbstract )
            .Select( aspectClass => aspectClass.TypeId.Id );

        var fabrics = pipeline.Fabrics ?? ImmutableArray<string>.Empty;
        var fabricsIds = fabrics.Select( fabric => compilation.GetTypeByMetadataName( fabric )?.GetSerializableTypeId().Id ).WhereNotNull();

        return aspectClassesIds.Concat( fabricsIds ).ToArray();
    }

    public async Task<IEnumerable<AspectDatabaseAspectInstance>> GetAspectInstancesAsync(
        ProjectKey projectKey,
        SerializableTypeId aspectClass,
        CancellationToken cancellationToken )
    {
        if ( await this.GetPipelineAndCompilationAsync( projectKey, cancellationToken ) is not var (designTimePipeline, compilation) )
        {
            return [];
        }

        if ( !this._aspectInstanceCache.TryGetValue( compilation, out var aspectInstances ) )
        {
            var designTimeConfiguration = await designTimePipeline.GetConfigurationAsync(
                PartialCompilation.CreateComplete( compilation ),
                false,
                AsyncExecutionContext.Get(),
                cancellationToken.ToTestable() );

            if ( !designTimeConfiguration.IsSuccessful )
            {
                this._aspectInstanceCache.TryAdd( compilation, ImmutableArray<IIntrospectionAspectInstance>.Empty );

                return [];
            }

            var pipeline = new IntrospectionAspectPipeline( designTimeConfiguration.Value.ServiceProvider, this._pipelineFactory.Domain, null );

            try
            {
                var result = await pipeline.ExecuteAsync(
                    PartialCompilation.CreateComplete( compilation ),
                    designTimeConfiguration.Value,
                    cancellationToken.ToTestable() );

                aspectInstances = result.AspectInstances.Where( i => !i.IsSkipped ).ToImmutableArray();
            }
            catch ( Exception ex )
            {
                DesignTimeExceptionHandler.ReportException( ex );

                aspectInstances = ImmutableArray<IIntrospectionAspectInstance>.Empty;
            }

            this._aspectInstanceCache.TryAdd( compilation, aspectInstances );
        }

        var compilationContext = CompilationContextFactory.GetInstance( compilation );
        var typeIdResolver = compilationContext.SerializableTypeIdResolver;

        if ( !typeIdResolver.TryResolveId( aspectClass, out var aspectTypeSymbol ) )
        {
            this._logger.Warning?.Log( $"Could not resolve '{aspectClass}'." );

            return [];
        }

        var aspectClassFullName = aspectTypeSymbol.GetReflectionFullName();

        var transformationAspectInstances = aspectInstances
            .Where( aspectInstance => aspectInstance.AspectClass.FullName == aspectClassFullName )
            .Select(
                aspectInstance => new AspectDatabaseAspectInstance(
                    aspectInstance.TargetDeclaration.ToSerializableId().Id,
                    aspectInstance.Advice
                        .SelectMany( advice => advice.Transformations )
                        .Select(
                            transformation => new AspectDatabaseAspectTransformation(
                                transformation.TargetDeclaration.ToSerializableId().Id,
                                transformation.ToString()! ) )
                        .ToArray() ) );

        static string? GetPredecessorFullName( IIntrospectionAspectPredecessor predecessor )
        {
            return predecessor switch
            {
                IIntrospectionAspectInstance predecessorAspect => predecessorAspect.AspectClass.Type.FullName,
                IIntrospectionFabric fabric => fabric.FullName,
                _ => null
            };
        }

        var predecessorAspectInstances = aspectInstances
            .Where( aspectInstance => aspectInstance.Predecessors.Any( predecessor => GetPredecessorFullName( predecessor.Instance ) == aspectClassFullName ) )
            .Select(
                aspectInstance => new AspectDatabaseAspectInstance(
                    aspectInstance.TargetDeclaration.ToSerializableId().Id,
                    new[]
                    {
                        new AspectDatabaseAspectTransformation(
                            aspectInstance.TargetDeclaration.ToSerializableId().Id,
                            $"Provide the '{aspectInstance.AspectClass}' aspect." )
                    } ) );

        return transformationAspectInstances.Concat( predecessorAspectInstances ).ToArray();
    }

    public event Action<ProjectKey> AspectClassesChanged
    {
        add => this._eventHub.AspectClassesChanged += value;
        remove => this._eventHub.AspectClassesChanged -= value;
    }

    public event Action<ProjectKey> AspectInstancesChanged
    {
        add => this._eventHub.AspectInstancesChanged += value;
        remove => this._eventHub.AspectInstancesChanged -= value;
    }
}