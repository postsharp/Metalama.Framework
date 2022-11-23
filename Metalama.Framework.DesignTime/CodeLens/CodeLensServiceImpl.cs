// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Introspection;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime.CodeLens;

internal class CodeLensServiceImpl : PreviewPipelineBasedService, ICodeLensServiceImpl
{
    private static readonly Task<CodeLensSummary> _noAspectResult = Task.FromResult( CodeLensSummary.NoAspect );

    private static readonly ImmutableArray<CodeLensDetailsHeader> _detailsHeaders = ImmutableArray.Create(
        new CodeLensDetailsHeader( "Aspect Class", "AspectShortName", width: 0.2 ),
        new CodeLensDetailsHeader( "Aspect Target", "TargetDeclaration", width: 0.2 ),
        new CodeLensDetailsHeader( "Aspect Origin", "Origin", width: 0.2 ),
        new CodeLensDetailsHeader( "Transformation", "Transformation", width: 0.4 ) );

    private readonly ILogger _logger;

    public CodeLensServiceImpl( GlobalServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "CodeLens" );
    }

    private bool TryGetSyntaxTree(
        ProjectKey projectKey,
        SerializableDeclarationId symbolId,
        [NotNullWhen( true )] out string? filePath,
        [NotNullWhen( true )] out ISymbol? symbol,
        [NotNullWhen( true )] out DesignTimeAspectPipeline? pipeline,
        [NotNullWhen( true )] out CompilationPipelineResult? pipelineResult )
    {
        filePath = null;
        symbol = null;
        pipelineResult = null;

        if ( !this.PipelineFactory.TryGetPipeline( projectKey, out pipeline ) )
        {
            this._logger.Trace?.Log( $"Cannot return code lens info for '{projectKey}' because the pipeline is not ready." );

            return false;
        }

        pipelineResult = pipeline.CompilationPipelineResult;

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

        filePath = symbol.GetPrimarySyntaxReference()?.SyntaxTree.FilePath;

        if ( filePath == null )
        {
            this._logger.Warning?.Log(
                $"Cannot return code lens info for symbol '{symbolId}' in '{projectKey}' because the symbol has no primary syntax tree." );

            return false;
        }

        return true;
    }

    public Task<CodeLensSummary> GetCodeLensSummaryAsync( ProjectKey projectKey, SerializableDeclarationId symbolId, CancellationToken cancellationToken )
    {
        if ( !this.TryGetSyntaxTree( projectKey, symbolId, out var filePath, out var symbol, out var pipeline, out var pipelineResult ) )
        {
            return _noAspectResult;
        }

        // Try to get a description from the symbol classifier.
        if ( TryGetSummaryFromSymbolClassifier( pipeline, symbol, out var summaryFromSymbolClassifier ) )
        {
            return Task.FromResult( summaryFromSymbolClassifier );
        }

        // If we have a plain method, display the number of target aspects.
        if ( !pipelineResult.SyntaxTreeResults.TryGetValue( filePath, out var syntaxTreeResult ) )
        {
            this._logger.Trace?.Log( $"Cannot return code lens info for symbol '{symbolId}' in '{projectKey}' because there is no result for this symbol." );

            return _noAspectResult;
        }

        var aspectInstances = syntaxTreeResult.AspectInstances.Where( i => i.TargetDeclarationId == symbolId ).Select( i => i.AspectClassFullName ).ToList();
        var transformations = syntaxTreeResult.Transformations.Where( t => t.TargetDeclarationId == symbolId ).Select( t => t.AspectClassFullName ).ToList();
        var distinctAspects = aspectInstances.Concat( transformations ).Distinct().ToList();
        var distinctAspectCount = distinctAspects.Count;

        this._logger.Trace?.Log( $"There are {distinctAspectCount} distinct aspect(s) affecting '{symbol}'." );

        var (text, tooltip) = distinctAspectCount switch
        {
            0 => ("no aspect", "This declaration is not affected by any aspect."),
            1 => ("1 aspect", $"This declaration is affected by the aspect '{distinctAspects.Single()}'."),
            _ => ($"{distinctAspectCount} aspects",
                  $"This declaration is affected by the aspects {string.Join( ", ", distinctAspects.SelectEnumerable( i => $"'{i}'" ) )}.")
        };

        return Task.FromResult( new CodeLensSummary( text, tooltip ) );
    }

    private static bool TryGetSummaryFromSymbolClassifier(
        DesignTimeAspectPipeline pipeline,
        ISymbol symbol,
        [NotNullWhen( true )] out CodeLensSummary? summary )
    {
        var symbolClassificationService = pipeline.ServiceProvider.GetRequiredService<CompilationServicesFactory>().GetInstance( pipeline.LastCompilation! ).SymbolClassificationService;

        string? executionScopeString = null;

        if ( symbolClassificationService.IsTemplate(  symbol ) )
        {
            executionScopeString = "template";
        }
        else
        {
            var executionScope = symbolClassificationService.GetExecutionScope( symbol );

            if ( executionScope != ExecutionScope.RunTime )
            {
                if ( executionScope == ExecutionScope.CompileTime )
                {
                    executionScopeString = "compile-time";
                }
                else if ( symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.AllInterfaces.Any( t => t.Name == nameof(IAspect) ) )
                {
                    {
                        summary = new CodeLensSummary( MetalamaStringFormatter.Format( $"aspect class" ) );

                        return true;
                    }
                }
                else
                {
                    executionScopeString = "both run-time and compile-time";
                }
            }
        }

        if ( executionScopeString != null )
        {
            {
                summary = new CodeLensSummary( MetalamaStringFormatter.Format( $"{executionScopeString} {symbol.Kind}" ) );

                return true;
            }
        }

        summary = null;

        return false;
    }

    public async Task<ICodeLensDetailsTable> GetCodeLensDetailsAsync(
        ProjectKey projectKey,
        SerializableDeclarationId symbolId,
        CancellationToken cancellationToken )
    {
        if ( !this.TryGetSyntaxTree( projectKey, symbolId, out var filePath, out _, out _, out _ ) )
        {
            return CodeLensDetailsTable.CreateError( "Something went wrong." );
        }

        // Execute the pipeline.
        var preparation = await this.PrepareExecutionAsync( projectKey, filePath, cancellationToken.ToTestable() );

        if ( !preparation.Success )
        {
            return CodeLensDetailsTable.CreateError( preparation.ErrorMessages! );
        }

        var pipeline = new IntrospectionAspectPipeline(
            preparation.ServiceProvider.AssertNotNull(),
            preparation.ServiceProvider!.Value.Global.GetRequiredService<DesignTimeAspectPipelineFactory>().Domain,
            null );

        var result = await pipeline.ExecuteAsync( preparation.PartialCompilation!, preparation.Configuration!, cancellationToken.ToTestable() );

        // Index aspects and transformations.
        var aspectInstances = result.AspectInstances.Where( i => i.TargetDeclaration.GetSymbol().TryGetSerializableId( out var id ) && id == symbolId )
            .ToDictionary( i => i, _ => new List<IIntrospectionTransformation>() );

        var transformations = result.Transformations.Where( t => t.TargetDeclaration.GetSymbol().TryGetSerializableId( out var id ) && id == symbolId );

        foreach ( var transformation in transformations )
        {
            var aspectInstance = transformation.Advice.AspectInstance;

            if ( !aspectInstances.TryGetValue( aspectInstance, out var transformationList ) )
            {
                aspectInstances[aspectInstance] = transformationList = new List<IIntrospectionTransformation>();
            }

            transformationList.Add( transformation );
        }

        // Create the logical table.
        List<CodeLensDetailsEntry> entries = new();

        CodeLensDetailsField CreateOriginField( IIntrospectionAspectInstance aspectInstance )
        {
            if ( aspectInstance.Predecessors.IsEmpty )
            {
                // This should not happen.
                return new CodeLensDetailsField( "-" );
            }

            var predecessor = aspectInstance.Predecessors[0];

            FormattableString text = predecessor.Kind switch
            {
                AspectPredecessorKind.Attribute => $"Custom attribute",
                AspectPredecessorKind.Fabric => $"Fabric '{((IIntrospectionFabric) predecessor.Instance).FullName}'",
                AspectPredecessorKind.Inherited => $"Inherited from '{((IIntrospectionAspectInstance) predecessor.Instance).TargetDeclaration}'",
                AspectPredecessorKind.ChildAspect => $"Child of '{((IIntrospectionAspectInstance) predecessor.Instance).TargetDeclaration}'",
                AspectPredecessorKind.RequiredAspect => $"Required by '{((IIntrospectionAspectInstance) predecessor.Instance).TargetDeclaration}'",
                _ => $""
            };

            return new CodeLensDetailsField( MetalamaStringFormatter.Format( text ) );
        }

        IIntrospectionAspectInstance? previousAspectInstance = null;

        void AddEntry( IIntrospectionAspectInstance aspectInstance, string transformation )
        {
            var aspectClass = aspectInstance.AspectClass.ShortName;
            var targetDeclaration = aspectInstance.TargetDeclaration.ToDisplayString();
            var origin = CreateOriginField( aspectInstance );

            // If we are repeating the previous aspect instance, add empty cells to ease reading.
            if ( previousAspectInstance == aspectInstance )
            {
                entries.Add(
                    new CodeLensDetailsEntry(
                        ImmutableArray.Create(
                            new CodeLensDetailsField( "" ),
                            new CodeLensDetailsField( "" ),
                            new CodeLensDetailsField( "" ),
                            new CodeLensDetailsField( transformation ) ) ) );
            }
            else
            {
                entries.Add(
                    new CodeLensDetailsEntry(
                        ImmutableArray.Create(
                            new CodeLensDetailsField( aspectClass ),
                            new CodeLensDetailsField( targetDeclaration ),
                            origin,
                            new CodeLensDetailsField( transformation ) ) ) );

                previousAspectInstance = aspectInstance;
            }
        }

        // First add aspects without transformations.
        foreach ( var aspectInstance in aspectInstances.Where( i => i.Value.Count == 0 ) )
        {
            var aspectInstanceTransformations =
                aspectInstance.Key.Advice.SelectMany( a => a.Transformations ).Select( t => t.TargetDeclaration ).Distinct().ToList();

            var transformationText = aspectInstanceTransformations.Count switch
            {
                0 => $"(The aspect does not provide any transformation.)",
                1 => $"(The aspect transforms 1 child declaration.)",
                _ => $"(The aspect transforms {aspectInstanceTransformations.Count} child declarations.)"
            };

            AddEntry( aspectInstance.Key, transformationText );
        }

        // Add transformations by execution order.
        foreach ( var transformation in aspectInstances.SelectMany( i => i.Value ).OrderBy( t => t.Order ) )
        {
            var aspectInstance = transformation.Advice.AspectInstance;

            var description = MetalamaStringFormatter.Format( transformation.Description );

            AddEntry( aspectInstance, description );
        }

        foreach ( var aspectInstance in aspectInstances.Where( a => a.Value.Count > 1 ) )
        {
            if ( aspectInstance.Key.TargetDeclaration.GetSymbol().TryGetSerializableId( out var id ) && id == symbolId )
            {
                var transformationsOnChildren = aspectInstance.Key.Advice.SelectMany( a => a.Transformations )
                    .Where(
                        t => !(t.TargetDeclaration.GetSymbol().TryGetSerializableId( out var transformedDeclarationId )
                               && transformedDeclarationId != symbolId) )
                    .Select( t => t.TargetDeclaration )
                    .Distinct()
                    .ToList();

                if ( transformationsOnChildren.Count > 0 )
                {
                    var transformationText = transformationsOnChildren.Count switch
                    {
                        1 => $"(The aspect also transforms 1 child declaration.)",
                        _ => $"(The aspect also transforms {transformationsOnChildren.Count} child declarations.)"
                    };

                    AddEntry( aspectInstance.Key, transformationText );
                }
            }
        }

        return new CodeLensDetailsTable( _detailsHeaders, entries.ToImmutableArray() );
    }
}