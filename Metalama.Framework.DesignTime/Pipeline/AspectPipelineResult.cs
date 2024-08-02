// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline;

/// <summary>
/// Caches the pipeline results for each syntax tree.
/// </summary>
internal sealed partial class AspectPipelineResult : ITransitiveAspectsManifest
{
    private static readonly ImmutableDictionary<string, SyntaxTreePipelineResult> _emptySyntaxTreeResults =
        ImmutableDictionary.Create<string, SyntaxTreePipelineResult>( StringComparer.Ordinal );

    private static readonly ImmutableDictionary<string, IntroducedSyntaxTree> _emptyIntroducedSyntaxTrees =
        ImmutableDictionary.Create<string, IntroducedSyntaxTree>( StringComparer.Ordinal );

    private static readonly ImmutableDictionaryOfHashSet<string, InheritableAspectInstance> _emptyInheritableAspects =
        ImmutableDictionaryOfHashSet<string, InheritableAspectInstance>.Create(
            StringComparer.Ordinal,
            InheritableAspectInstance.ByTargetComparer.Instance );

    private static readonly ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions> _emptyInheritableOptions
        = ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions>.Empty;

    private static readonly ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation> _emptyAnnotations =
        ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation>.Empty;

    private static long _nextId;
    private readonly long _id = Interlocked.Increment( ref _nextId );

    private bool IsEmpty
        => this.SyntaxTreeResults.IsEmpty && this.IntroducedSyntaxTrees.IsEmpty && this.ReferenceValidators.IsEmpty && this._inheritableAspects.IsEmpty;

    public DesignTimeReferenceValidatorCollection ReferenceValidators { get; } = DesignTimeReferenceValidatorCollection.Empty;

    public ImmutableDictionary<string, IntroducedSyntaxTree> IntroducedSyntaxTrees { get; } = _emptyIntroducedSyntaxTrees;

    /// <summary>
    /// Gets a maps if the syntax tree name to the pipeline result for this syntax tree.
    /// </summary>
    public ImmutableDictionary<string, SyntaxTreePipelineResult> SyntaxTreeResults { get; } = _emptySyntaxTreeResults;

    /// <summary>
    /// List of SyntaxTreeResult that have been invalidated.
    /// </summary>
    private readonly ImmutableDictionary<string, SyntaxTreePipelineResult> _invalidSyntaxTreeResults = _emptySyntaxTreeResults;

    private readonly ImmutableDictionaryOfHashSet<string, InheritableAspectInstance> _inheritableAspects = _emptyInheritableAspects;

    public ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions> InheritableOptions { get; } = _emptyInheritableOptions;

    public ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation> Annotations { get; } = _emptyAnnotations;

    public ulong AspectInstancesHashCode { get; }

    private byte[]? _serializedTransitiveAspectManifest;

    private AspectPipelineResult(
        AspectPipelineConfiguration? configuration,
        ImmutableDictionary<string, SyntaxTreePipelineResult> syntaxTreeResults,
        ImmutableDictionary<string, SyntaxTreePipelineResult> invalidSyntaxTreeResults,
        ImmutableDictionary<string, IntroducedSyntaxTree> introducedSyntaxTrees,
        ImmutableDictionaryOfHashSet<string, InheritableAspectInstance> inheritableAspects,
        DesignTimeReferenceValidatorCollection referenceValidators,
        ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions> inheritableOptions,
        ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation> annotations,
        ulong aspectInstancesHashCode )
    {
        this.SyntaxTreeResults = syntaxTreeResults;
        this._invalidSyntaxTreeResults = invalidSyntaxTreeResults;
        this.IntroducedSyntaxTrees = introducedSyntaxTrees;
        this._inheritableAspects = inheritableAspects;
        this.InheritableOptions = inheritableOptions;
        this.ReferenceValidators = referenceValidators;
        this.Configuration = configuration;
        this.Annotations = annotations;
        this.AspectInstancesHashCode = aspectInstancesHashCode;

        Logger.DesignTime.Trace?.Log(
            $"CompilationPipelineResult {this._id} created with {this.SyntaxTreeResults.Count} syntax trees and {this._invalidSyntaxTreeResults.Count} introduced syntax trees." );

        if ( !this.IsEmpty && configuration == null )
        {
            throw new AssertionFailedException();
        }
    }

    internal AspectPipelineResult() { }

    /// <summary>
    /// Gets the pipeline configuration, or potentially <c>null</c>  if the current <see cref="AspectPipelineResult"/> is empty.
    /// </summary>
    public AspectPipelineConfiguration? Configuration { get; }

    /// <summary>
    /// Updates cache with a <see cref="DesignTimePipelineExecutionResult"/> that includes results for several syntax trees.
    /// </summary>
    internal AspectPipelineResult Update(
        PartialCompilation compilation,
        DesignTimeProjectVersion projectVersion,
        DesignTimePipelineExecutionResult pipelineResults,
        AspectPipelineConfiguration configuration )
    {
        Logger.DesignTime.Trace?.Log( $"CompilationPipelineResult.Update( id = {this._id} )" );

        var (resultsByTree, externalValidators) = SplitResultsByTree( compilation, pipelineResults );

        var syntaxTreeResultBuilder = this.SyntaxTreeResults.ToBuilder();

        ImmutableDictionary<string, IntroducedSyntaxTree>.Builder? introducedSyntaxTreeBuilder = null;
        ImmutableDictionaryOfHashSet<string, InheritableAspectInstance>.Builder? inheritableAspectsBuilder = null;
        DesignTimeReferenceValidatorCollection.Builder? validatorsBuilder = null;
        ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions>.Builder? inheritableOptionsBuilder = null;
        ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation>.Builder? annotationsBuilder = null;
        var aspectInstancesHashCode = this.AspectInstancesHashCode;

        foreach ( var result in resultsByTree )
        {
            var filePath = result.SyntaxTreePath ?? "";

            // Un-index the old tree.
            if ( syntaxTreeResultBuilder.TryGetValue( filePath, out var oldSyntaxTreeResult ) ||
                 this._invalidSyntaxTreeResults.TryGetValue( filePath, out oldSyntaxTreeResult ) )
            {
                if ( !oldSyntaxTreeResult.Introductions.IsEmpty )
                {
                    introducedSyntaxTreeBuilder ??= this.IntroducedSyntaxTrees.ToBuilder();

                    foreach ( var introducedTree in oldSyntaxTreeResult.Introductions )
                    {
                        Logger.DesignTime.Trace?.Log(
                            $"CompilationPipelineResult.Update( id = {this._id} ): removing introduced tree '{introducedTree.Name}'." );

                        introducedSyntaxTreeBuilder.Remove( introducedTree.Name );
                    }
                }

                if ( !oldSyntaxTreeResult.InheritableAspects.IsEmpty )
                {
                    inheritableAspectsBuilder ??= this._inheritableAspects.ToBuilder();

                    foreach ( var x in oldSyntaxTreeResult.InheritableAspects )
                    {
                        Logger.DesignTime.Trace?.Log(
                            $"CompilationPipelineResult.Update( id = {this._id} ): removing inheritable aspect of type '{x.AspectClass.ShortName}'." );

                        inheritableAspectsBuilder.Remove( x.AspectClass.FullName, x );
                    }
                }

                if ( !oldSyntaxTreeResult.ReferenceValidators.IsEmpty )
                {
                    validatorsBuilder ??= this.ReferenceValidators.ToBuilder();

                    foreach ( var validator in oldSyntaxTreeResult.ReferenceValidators )
                    {
                        Logger.DesignTime.Trace?.Log(
                            $"CompilationPipelineResult.Update( id = {this._id} ): removing validator `{validator}` from syntax tree '{filePath}'." );

                        validatorsBuilder.Remove( validator );
                    }
                }

                if ( !oldSyntaxTreeResult.InheritableOptions.IsDefault )
                {
                    inheritableOptionsBuilder ??= this.InheritableOptions.ToBuilder();

                    foreach ( var optionItem in oldSyntaxTreeResult.InheritableOptions )
                    {
                        Logger.DesignTime.Trace?.Log(
                            $"CompilationPipelineResult.Update( id = {this._id} ): removing inheritable option of type `{optionItem.Key.OptionType}` on `{optionItem.Key.DeclarationId}` from syntax tree '{filePath}'." );

                        inheritableOptionsBuilder.Remove( optionItem.Key );
                    }
                }

                if ( !oldSyntaxTreeResult.Annotations.IsEmpty )
                {
                    annotationsBuilder ??= this.Annotations.ToBuilder();

                    foreach ( var annotation in oldSyntaxTreeResult.Annotations )
                    {
                        annotationsBuilder.Remove( annotation.Key, annotation );
                    }
                }

                aspectInstancesHashCode ^= oldSyntaxTreeResult.AspectInstancesHashCode;
            }

            // Index the new tree.
            if ( !result.Introductions.IsEmpty )
            {
                introducedSyntaxTreeBuilder ??= this.IntroducedSyntaxTrees.ToBuilder();

                foreach ( var introducedTree in result.Introductions )
                {
                    Logger.DesignTime.Trace?.Log(
                        $"CompilationPipelineResult.Update( id = {this._id} ): adding introduced syntax tree '{introducedTree.Name}'." );

                    introducedSyntaxTreeBuilder.Add( introducedTree.Name, introducedTree );
                }
            }

            if ( !result.InheritableAspects.IsEmpty )
            {
                inheritableAspectsBuilder ??= this._inheritableAspects.ToBuilder();

                foreach ( var x in result.InheritableAspects )
                {
                    Logger.DesignTime.Trace?.Log(
                        $"CompilationPipelineResult.Update( id = {this._id} ): adding inheritable aspect of type '{x.AspectClass.ShortName}'." );

                    inheritableAspectsBuilder.Add( x.AspectClass.FullName, x );
                }
            }

            if ( !result.ReferenceValidators.IsDefaultOrEmpty )
            {
                validatorsBuilder ??= this.ReferenceValidators.ToBuilder();

                foreach ( var validator in result.ReferenceValidators )
                {
                    Logger.DesignTime.Trace?.Log( $"CompilationPipelineResult.Update( id = {this._id} ): adding validator `{validator}` to '{filePath}'." );
                    validatorsBuilder.Add( validator );
                }
            }

            if ( !result.InheritableOptions.IsDefaultOrEmpty )
            {
                inheritableOptionsBuilder ??= this.InheritableOptions.ToBuilder();

                foreach ( var optionItem in result.InheritableOptions )
                {
                    Logger.DesignTime.Trace?.Log(
                        $"CompilationPipelineResult.Update( id = {this._id} ): adding inheritable options of type `{optionItem.Key.OptionType}`." );

                    inheritableOptionsBuilder.Add( optionItem.Key, optionItem.Options );
                }
            }

            if ( !result.Annotations.IsEmpty )
            {
                annotationsBuilder ??= this.Annotations.ToBuilder();

                foreach ( var annotationGroup in result.Annotations )
                {
                    annotationsBuilder.Add( annotationGroup.Key, annotationGroup );
                }
            }

            aspectInstancesHashCode ^= result.AspectInstancesHashCode;

            syntaxTreeResultBuilder[filePath] = result;
        }

        // Make immutable and return.
        var introducedTrees = introducedSyntaxTreeBuilder?.ToImmutable() ?? this.IntroducedSyntaxTrees;
        var inheritableAspects = inheritableAspectsBuilder?.ToImmutable() ?? this._inheritableAspects;

        if ( externalValidators != null )
        {
            validatorsBuilder ??= this.ReferenceValidators.ToBuilder();

            foreach ( var externalValidator in externalValidators )
            {
                if (externalValidator == null)
                {
                    continue;
                }

                validatorsBuilder.Add( externalValidator );
            }
        }

        var validators = validatorsBuilder?.ToImmutable( projectVersion.ReferencedValidatorCollections )
                         ?? this.ReferenceValidators.WithChildCollections( projectVersion.ReferencedValidatorCollections );

        var inheritableOptions = inheritableOptionsBuilder?.ToImmutable() ?? this.InheritableOptions;
        var annotations = annotationsBuilder?.ToImmutable() ?? this.Annotations;

        return new AspectPipelineResult(
            configuration,
            syntaxTreeResultBuilder.ToImmutable(),
            ImmutableDictionary<string, SyntaxTreePipelineResult>.Empty,
            introducedTrees,
            inheritableAspects,
            validators,
            inheritableOptions,
            annotations,
            aspectInstancesHashCode );
    }

    /// <summary>
    /// Splits a <see cref="DesignTimePipelineExecutionResult"/>, which includes data for several syntax trees, into
    /// a list of <see cref="SyntaxTreePipelineResult"/> which each have information related to a single syntax tree.
    /// </summary>
    private static (IEnumerable<SyntaxTreePipelineResult> Results, IReadOnlyList<DesignTimeReferenceValidatorInstance>? ExternalValidators) SplitResultsByTree(
        PartialCompilation compilation,
        DesignTimePipelineExecutionResult pipelineResults )
    {
        SyntaxTreePipelineResult.Builder? emptySyntaxTreeResult = null;

        var resultBuilders = pipelineResults
            .InputSyntaxTrees
            .ToDictionary( r => r.Key, syntaxTree => new SyntaxTreePipelineResult.Builder( syntaxTree.Value ) );

        // TODO: This selects a single syntax tree and uses it as input tree of all "independent" introduced syntax trees.
        var inputSyntaxTreeForDetached = pipelineResults.InputSyntaxTrees.First().Value;

        List<DesignTimeReferenceValidatorInstance>? externalValidators = null;

        // Split diagnostic by syntax tree.
        foreach ( var diagnostic in pipelineResults.Diagnostics.ReportedDiagnostics )
        {
            var filePath = diagnostic.Location.SourceTree?.FilePath;

            if ( filePath != null )
            {
                if ( resultBuilders.TryGetValue( filePath, out var builder ) )
                {
                    builder.Diagnostics ??= ImmutableArray.CreateBuilder<Diagnostic>();
                    builder.Diagnostics.Add( diagnostic );
                }
                else
                {
                    // This can happen when a CS error is reported in the aspect. These errors can be ignored.
                }
            }
        }

        // Split suppressions by syntax tree.
        foreach ( var suppression in pipelineResults.Diagnostics.DiagnosticSuppressions )
        {
            void AddSuppression( string? path )
            {
                if ( !string.IsNullOrEmpty( path ) )
                {
                    if ( resultBuilders.TryGetValue( path, out var builder ) )
                    {
                        builder.Suppressions ??= ImmutableArray.CreateBuilder<CacheableScopedSuppression>();
                        builder.Suppressions.Add( new CacheableScopedSuppression( suppression ) );
                    }
                    else
                    {
                        // This can happen when a suppression is applied to an aspect that is in a different compilation, e.g. with [IntroduceDependency].
                    }
                }
            }

            var declaringSyntaxes = suppression.Declaration.GetDeclaringSyntaxReferences();

            switch ( declaringSyntaxes.Length )
            {
                case 0:
                    continue;

                case 1:
                    AddSuppression( declaringSyntaxes[0].SyntaxTree.FilePath );

                    break;

                default:
                    foreach ( var filePath in declaringSyntaxes.Select( p => p.SyntaxTree.FilePath ).Distinct() )
                    {
                        AddSuppression( filePath );
                    }

                    break;
            }
        }

        // Split introductions by original syntax tree.
        foreach ( var introduction in pipelineResults.IntroducedSyntaxTrees )
        {
            var syntaxTree = introduction.SourceSyntaxTree ?? inputSyntaxTreeForDetached;
            var filePath = syntaxTree.FilePath;

            if ( !resultBuilders.TryGetValue( filePath, out var builder ) )
            {
                // This happens when the source tree is not dirty, so it's not part of the PartialCompilation.
                builder = resultBuilders[filePath] = new SyntaxTreePipelineResult.Builder( syntaxTree );
            }

            builder.Introductions ??= ImmutableArray.CreateBuilder<IntroducedSyntaxTree>();

            if ( introduction.SourceSyntaxTree == null )
            {
                // TODO: This is a temporary hack until we have a proper way to handle "independent" introduced syntax trees.
                builder.Introductions.Add( new IntroducedSyntaxTree( introduction.Name, inputSyntaxTreeForDetached, introduction.GeneratedSyntaxTree ) );

                continue;
            }

            builder.Introductions.Add( introduction );
        }

        var compilationContext = compilation.CompilationContext;

        // Split inheritable aspects by syntax tree.
        foreach ( var inheritableAspectInstance in pipelineResults.InheritableAspects )
        {
            var syntaxTree = inheritableAspectInstance.TargetDeclaration.GetPrimarySyntaxTree( compilationContext );

            if ( syntaxTree == null )
            {
                continue;
            }

            var filePath = syntaxTree.FilePath;
            var builder = resultBuilders[filePath];
            builder.InheritableAspects ??= ImmutableArray.CreateBuilder<InheritableAspectInstance>();
            builder.InheritableAspects.Add( inheritableAspectInstance );
        }

        // Split validators by syntax tree.
        foreach ( var validator in pipelineResults.ReferenceValidators )
        {
            var syntaxTree = validator.ValidatedDeclaration.GetPrimarySyntaxTree();

            if ( syntaxTree == null )
            {
                continue;
            }

            var filePath = syntaxTree.FilePath;

            var validatedDeclarationSymbol = validator.ValidatedDeclaration.GetSymbol();

            if ( validatedDeclarationSymbol != null )
            {
                var designTimeValidator = new DesignTimeReferenceValidatorInstance(
                    validatedDeclarationSymbol,
                    validator.Properties.ReferenceKinds,
                    validator.Properties.IncludeDerivedTypes,
                    validator.Driver,
                    validator.Implementation,
                    validator.DiagnosticSourceDescription,
                    validator.Granularity );

                if ( resultBuilders.TryGetValue( filePath, out var builder ) )
                {
                    builder.Validators ??= ImmutableArray.CreateBuilder<DesignTimeReferenceValidatorInstance>();
                    builder.Validators.Add( designTimeValidator );
                }
                else
                {
                    // This happens with cross-project validators i.e. validator
                    externalValidators ??= new List<DesignTimeReferenceValidatorInstance>();
                    externalValidators.Add( designTimeValidator );
                }
            }
            else
            {
                // TODO: validating a declaration that is not backed by a symbol is not supported at design time at the moment.
            }
        }

        // Split aspect instances by syntax tree.
        foreach ( var aspectInstance in pipelineResults.AspectInstances )
        {
            var syntaxTree = aspectInstance.TargetDeclaration.GetPrimarySyntaxTree( compilationContext );

            // No continue here to handle even aspect instances without a syntax tree.
            if ( syntaxTree == null && !resultBuilders.ContainsKey( string.Empty ) )
            {
                resultBuilders.Add( string.Empty, new SyntaxTreePipelineResult.Builder( null ) );
            }

            var targetDeclarationId = aspectInstance.TargetDeclaration.ToSerializableId();
            SerializableDeclarationId? predecessorDeclarationId = null;

            if ( aspectInstance.Predecessors is [var predecessor, ..] )
            {
                var reflectionMapper = ((ICompilationServices) compilationContext).ReflectionMapper;

                var predecessorDeclarationSymbol = predecessor.Instance switch
                {
                    IAspectInstance predecessorAspect => reflectionMapper.GetTypeSymbol( predecessorAspect.Aspect.GetType() ),

                    // Can't use fabricInstance.Fabric.GetType() here, because for type fabrics,
                    // we need the original type (e.g. C.Fabric), not the rewritten type (e.g. C_Fabric).
                    IFabricInstance fabricInstance => compilationContext.Compilation.GetTypeByMetadataName(
                        ((IFabricInstanceInternal) fabricInstance).FabricTypeFullName ),
                    _ => null
                };

                predecessorDeclarationId = predecessorDeclarationSymbol?.GetSerializableId();
            }

            var filePath = syntaxTree?.FilePath ?? string.Empty;
            var builder = resultBuilders[filePath];
            builder.AspectInstances ??= ImmutableArray.CreateBuilder<DesignTimeAspectInstance>();

            builder.AspectInstances.Add(
                new DesignTimeAspectInstance(
                    targetDeclarationId,
                    predecessorDeclarationId,
                    aspectInstance.AspectClass.FullName,
                    aspectInstance.IsSkipped ) );
        }

        // Split transformations by syntax tree.
        foreach ( var transformation in pipelineResults.Transformations )
        {
            var targetSymbol = transformation.TargetDeclaration.GetSymbol();
            var primarySyntaxReference = targetSymbol?.GetPrimarySyntaxReference();

            var filePath = primarySyntaxReference?.SyntaxTree.FilePath;

            if ( filePath == null || !resultBuilders.ContainsKey( filePath ) )
            {
                filePath = inputSyntaxTreeForDetached.FilePath;
            }

            var builder = resultBuilders[filePath];
            builder.Transformations ??= ImmutableArray.CreateBuilder<DesignTimeTransformation>();

            builder.Transformations.Add(
                new DesignTimeTransformation(
                    transformation.TargetDeclaration.ToSerializableId(),
                    transformation.AspectClass.FullName,
                    MetalamaStringFormatter.Format( transformation.ToDisplayString() ) ) );
        }

        // Split options by syntax tree.
        foreach ( var optionItem in pipelineResults.InheritableOptions )
        {
            SyntaxTreePipelineResult.Builder builder;
            var syntaxTreePath = optionItem.Key.SyntaxTreePath;

            if ( syntaxTreePath != null )
            {
                builder = resultBuilders[syntaxTreePath];
            }
            else
            {
                builder = emptySyntaxTreeResult ??= new SyntaxTreePipelineResult.Builder( null );
            }

            builder.InheritableOptions ??= ImmutableArray.CreateBuilder<InheritableOptionsInstance>();
            builder.InheritableOptions.Add( new InheritableOptionsInstance( optionItem.Key, optionItem.Value ) );
        }

        // Split annotations by syntax tree.
        foreach ( var annotationsOnDeclaration in pipelineResults.Annotations )
        {
            // Annotations in AspectPipelineResults are only used for the cross-project scenario, so we only index exported annotations.
            var exportedAnnotations = annotationsOnDeclaration
                .Where( x => x.Export )
                .Select( x => x.Annotation )
                .ToImmutableArray();

            if ( exportedAnnotations.IsEmpty )
            {
                continue;
            }

            var syntaxTree = annotationsOnDeclaration.Key.GetPrimarySyntaxTree( compilation.CompilationContext );

            SyntaxTreePipelineResult.Builder builder;

            if ( syntaxTree == null )
            {
                builder = emptySyntaxTreeResult ??= new SyntaxTreePipelineResult.Builder( null );
            }
            else
            {
                var filePath = syntaxTree.FilePath;
                builder = resultBuilders[filePath];
            }

            builder.Annotations ??= ImmutableDictionaryOfArray<SerializableDeclarationId, IAnnotation>.CreateBuilder();
            builder.Annotations.Add( annotationsOnDeclaration.Key.ToSerializableId(), exportedAnnotations );
        }

        // Add syntax trees with empty output to it gets cached too.
        var inputTreesWithoutOutput = compilation.SyntaxTrees.ToBuilder();

        foreach ( var path in resultBuilders.Keys )
        {
            inputTreesWithoutOutput.Remove( path );
        }

        foreach ( var empty in inputTreesWithoutOutput )
        {
            resultBuilders.Add( empty.Key, new SyntaxTreePipelineResult.Builder( empty.Value ) );
        }

        if ( emptySyntaxTreeResult != null )
        {
            resultBuilders[""] = emptySyntaxTreeResult;
        }

        return (resultBuilders.SelectAsReadOnlyCollection( b => b.Value.ToImmutable( compilation.Compilation ) ), externalValidators);
    }

    public Invalidator ToInvalidator() => new( this );

    public bool IsSyntaxTreeDirty( SyntaxTree syntaxTree ) => !this.SyntaxTreeResults.ContainsKey( syntaxTree.FilePath );

    public IEnumerable<string> InheritableAspectTypes => this._inheritableAspects.Keys;

    public IEnumerable<InheritableAspectInstance> GetInheritableAspects( string aspectType ) => this._inheritableAspects[aspectType];

    // At design time, cross-project reference validators are not added to the main pipeline. Instead, the validator provider recursively includes
    // the providers of referenced projects. However cross-project references are still used for PE references.
    ImmutableArray<TransitiveValidatorInstance> ITransitiveAspectsManifest.ReferenceValidators => ImmutableArray<TransitiveValidatorInstance>.Empty;

    public byte[] GetSerializedTransitiveAspectManifest( in ProjectServiceProvider serviceProvider, Compilation compilation )
    {
        if ( this._serializedTransitiveAspectManifest == null )
        {
            var manifest = TransitiveAspectsManifest.Create(
                this._inheritableAspects.SelectMany( g => g ).ToImmutableArray(),
                this.ReferenceValidators.ToTransitiveValidatorInstances( CompilationContextFactory.GetInstance( compilation ) ),
                this.InheritableOptions,
                this.Annotations );

            this._serializedTransitiveAspectManifest = manifest.ToBytes( serviceProvider, compilation );
        }

        return this._serializedTransitiveAspectManifest;
    }
}