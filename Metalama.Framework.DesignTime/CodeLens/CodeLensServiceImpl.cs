// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime.CodeLens;

internal class CodeLensServiceImpl : ICodeLensServiceImpl
{
    private static readonly Task<CodeLensSummary> _noAspectResult = Task.FromResult( CodeLensSummary.NoAspect );

    private static readonly ImmutableArray<CodeLensDetailsHeader> _detailsHeaders = ImmutableArray.Create(
        new CodeLensDetailsHeader( "Target declaration", "TargetDeclaration", width: 300 ),
        new CodeLensDetailsHeader( "Aspect name", "AspectShortName", width: 300 ) );

    private readonly ILogger _logger;

    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;

    public CodeLensServiceImpl( IServiceProvider serviceProvider )
    {
        this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeLens" );
    }

    private bool TryGetSyntaxTreeResults(
        ProjectKey projectKey,
        SerializableDeclarationId symbolId,
        [NotNullWhen( true )] out SyntaxTreePipelineResult? syntaxTreeResult,
        [NotNullWhen( true )] out ISymbol? symbol )
    {
        syntaxTreeResult = null;
        symbol = null;

        if ( !this._pipelineFactory.TryGetPipeline( projectKey, out var pipeline ) )
        {
            this._logger.Trace?.Log( $"Cannot return code lens info for '{projectKey}' because the pipeline is not ready." );

            return false;
        }

        var pipelineResult = pipeline.CompilationPipelineResult;

        if ( pipelineResult == null )
        {
            this._logger.Trace?.Log( $"Cannot return code lens info for '{projectKey}' because the pipeline has not been executed yet." );

            return false;
        }

        var compilation = pipeline.LastCompilation;

        if ( compilation == null )
        {
            this._logger.Trace?.Log( $"Cannot return code lens info for '{projectKey}' because the pipeline has no active compilation." );

            return false;
        }

        var nullableSymbol = symbolId.Resolve( compilation );

        if ( nullableSymbol == null )
        {
            this._logger.Warning?.Log( $"Cannot return code lens info for symbol '{symbolId}' in '{projectKey}' because the symbol could not be resolved." );

            return false;
        }

        symbol = nullableSymbol;

        var filePath = symbol.GetPrimarySyntaxReference()?.SyntaxTree.FilePath;

        if ( filePath == null )
        {
            this._logger.Warning?.Log(
                $"Cannot return code lens info for symbol '{symbolId}' in '{projectKey}' because the symbol has no primary syntax tree." );

            return false;
        }

        if ( !pipelineResult.SyntaxTreeResults.TryGetValue( filePath, out syntaxTreeResult ) )
        {
            this._logger.Trace?.Log( $"Cannot return code lens info for symbol '{symbolId}' in '{projectKey}' because there is no result for this symbol." );

            return false;
        }

        return true;
    }

    public Task<CodeLensSummary> GetCodeLensInfoAsync( ProjectKey projectKey, SerializableDeclarationId symbolId, CancellationToken cancellationToken )
    {
        if ( !this.TryGetSyntaxTreeResults( projectKey, symbolId, out var syntaxTreeResult, out var symbol ) )
        {
            return _noAspectResult;
        }

        var aspectInstanceCount = syntaxTreeResult.AspectInstances.Count( i => i.TargetDeclarationId == symbolId );
        var transformationCount = syntaxTreeResult.Transformations.Count( t => t.TargetDeclarationId == symbolId );
        
        this._logger.Trace?.Log( $"There are {aspectInstanceCount} aspects and {transformationCount} transformations on '{symbol}'." );
        static string GetPlural( int count ) => count > 1 ? "s" : "";

        var summary = $"{aspectInstanceCount} aspect{GetPlural( aspectInstanceCount )}, {transformationCount} transformation{GetPlural( transformationCount )}";

        return Task.FromResult( new CodeLensSummary( summary, "" ) );
    }

    public Task<ICodeLensDetailsTable> GetCodeLensDetailsAsync( ProjectKey projectKey, SerializableDeclarationId symbolId, CancellationToken cancellationToken )
    {
        if ( !this.TryGetSyntaxTreeResults( projectKey, symbolId, out var syntaxTreeResult, out var symbol ) )
        {
            return Task.FromResult<ICodeLensDetailsTable>( CodeLensDetailsTable.Empty );
        }

        var aspectInstances = syntaxTreeResult.AspectInstances.Where( i => i.TargetDeclarationId == symbolId ).ToList();

        var entries = aspectInstances.SelectArray(
                i => new CodeLensDetailsEntry(
                    ImmutableArray.Create( new CodeLensDetailsField( symbol.ToDisplayString() ), new CodeLensDetailsField( i.AspectClassShortName ) ),
                    "" ) )
            .ToImmutableArray();

        return Task.FromResult<ICodeLensDetailsTable>( new CodeLensDetailsTable( _detailsHeaders, entries ) );
    }
}