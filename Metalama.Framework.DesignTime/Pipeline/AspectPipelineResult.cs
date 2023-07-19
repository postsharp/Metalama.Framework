// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline
{
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

        private static long _nextId;
        private readonly long _id = Interlocked.Increment( ref _nextId );

        private bool IsEmpty
            => this.SyntaxTreeResults.IsEmpty && this.IntroducedSyntaxTrees.IsEmpty && this.ReferenceValidators.IsEmpty && this._inheritableAspects.IsEmpty;

        internal DesignTimeReferenceValidatorCollection ReferenceValidators { get; } = DesignTimeReferenceValidatorCollection.Empty;

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
        private byte[]? _serializedTransitiveAspectManifest;

        private AspectPipelineResult(
            ImmutableDictionary<string, SyntaxTreePipelineResult> syntaxTreeResults,
            ImmutableDictionary<string, SyntaxTreePipelineResult> invalidSyntaxTreeResults,
            ImmutableDictionary<string, IntroducedSyntaxTree> introducedSyntaxTrees,
            ImmutableDictionaryOfHashSet<string, InheritableAspectInstance> inheritableAspects,
            DesignTimeReferenceValidatorCollection referenceValidators,
            AspectPipelineConfiguration? configuration )
        {
            this.SyntaxTreeResults = syntaxTreeResults;
            this._invalidSyntaxTreeResults = invalidSyntaxTreeResults;
            this.IntroducedSyntaxTrees = introducedSyntaxTrees;
            this._inheritableAspects = inheritableAspects;
            this.ReferenceValidators = referenceValidators;
            this.Configuration = configuration;

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
            DesignTimePipelineExecutionResult pipelineResults,
            AspectPipelineConfiguration configuration )
        {
            Logger.DesignTime.Trace?.Log( $"CompilationPipelineResult.Update( id = {this._id} )" );

            var resultsByTree = SplitResultsByTree( compilation, pipelineResults );

            var syntaxTreeResultBuilder = this.SyntaxTreeResults.ToBuilder();

            ImmutableDictionary<string, IntroducedSyntaxTree>.Builder? introducedSyntaxTreeBuilder = null;
            ImmutableDictionaryOfHashSet<string, InheritableAspectInstance>.Builder? inheritableAspectsBuilder = null;
            DesignTimeReferenceValidatorCollection.Builder? validatorsBuilder = null;

            foreach ( var result in resultsByTree )
            {
                var filePath = result.SyntaxTree.FilePath;

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
                                $"CompilationPipelineResult.Update( id = {this._id} ): removing inheritable aspect of type '{x.AspectType}'." );

                            inheritableAspectsBuilder.Remove( x.AspectType, x.AspectInstance );
                        }
                    }

                    if ( !oldSyntaxTreeResult.ReferenceValidators.IsEmpty )
                    {
                        validatorsBuilder ??= this.ReferenceValidators.ToBuilder();

                        foreach ( var validator in oldSyntaxTreeResult.ReferenceValidators )
                        {
                            Logger.DesignTime.Trace?.Log( $"CompilationPipelineResult.Update( id = {this._id} ): removing validator." );
                            validatorsBuilder.Remove( validator );
                        }
                    }
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
                            $"CompilationPipelineResult.Update( id = {this._id} ): adding inheritable aspect of type '{x.AspectType}'." );

                        inheritableAspectsBuilder.Add( x.AspectType, x.AspectInstance );
                    }
                }

                if ( !result.ReferenceValidators.IsDefaultOrEmpty )
                {
                    validatorsBuilder ??= this.ReferenceValidators.ToBuilder();

                    foreach ( var validator in result.ReferenceValidators )
                    {
                        Logger.DesignTime.Trace?.Log( $"CompilationPipelineResult.Update( id = {this._id} ): adding validator." );
                        validatorsBuilder.Add( validator );
                    }
                }

                syntaxTreeResultBuilder[filePath] = result;
            }

            var introducedTrees = introducedSyntaxTreeBuilder?.ToImmutable() ?? this.IntroducedSyntaxTrees;
            var inheritableAspects = inheritableAspectsBuilder?.ToImmutable() ?? this._inheritableAspects;
            var validators = validatorsBuilder?.ToImmutable() ?? this.ReferenceValidators;

            return new AspectPipelineResult(
                syntaxTreeResultBuilder.ToImmutable(),
                ImmutableDictionary<string, SyntaxTreePipelineResult>.Empty,
                introducedTrees,
                inheritableAspects,
                validators,
                configuration );
        }

        /// <summary>
        /// Splits a <see cref="DesignTimePipelineExecutionResult"/>, which includes data for several syntax trees, into
        /// a list of <see cref="SyntaxTreePipelineResult"/> which each have information related to a single syntax tree.
        /// </summary>
        private static IEnumerable<SyntaxTreePipelineResult> SplitResultsByTree(
            PartialCompilation compilation,
            DesignTimePipelineExecutionResult pipelineResults )
        {
            var resultBuilders = pipelineResults
                .InputSyntaxTrees
                .ToDictionary( r => r.Key, syntaxTree => new SyntaxTreePipelineResult.Builder( syntaxTree.Value ) );

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
                        var builder = resultBuilders[path];
                        builder.Suppressions ??= ImmutableArray.CreateBuilder<CacheableScopedSuppression>();
                        builder.Suppressions.Add( new CacheableScopedSuppression( suppression ) );
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
                var filePath = introduction.SourceSyntaxTree.FilePath;
                var builder = resultBuilders[filePath];
                builder.Introductions ??= ImmutableArray.CreateBuilder<IntroducedSyntaxTree>();
                builder.Introductions.Add( introduction );
            }

            // Split inheritable aspects by syntax tree.
            foreach ( var inheritableAspectInstance in pipelineResults.InheritableAspects )
            {
                var syntaxTree = inheritableAspectInstance.TargetDeclaration.GetPrimarySyntaxTree( compilation.CompilationContext );

                if ( syntaxTree == null )
                {
                    continue;
                }

                var filePath = syntaxTree.FilePath;
                var builder = resultBuilders[filePath];
                builder.InheritableAspects ??= ImmutableArray.CreateBuilder<(string, InheritableAspectInstance)>();
                builder.InheritableAspects.Add( (inheritableAspectInstance.Aspect.GetType().FullName.AssertNotNull(), inheritableAspectInstance) );
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
                var builder = resultBuilders[filePath];
                builder.Validators ??= ImmutableArray.CreateBuilder<DesignTimeReferenceValidatorInstance>();

                var validatedDeclarationSymbol = validator.ValidatedDeclaration.GetSymbol();

                if ( validatedDeclarationSymbol != null )
                {
                    builder.Validators.Add(
                        new DesignTimeReferenceValidatorInstance(
                            validatedDeclarationSymbol,
                            validator.ReferenceKinds,
                            validator.Driver,
                            validator.Implementation ) );
                }
                else
                {
                    // TODO: validating a declaration that is not backed by a symbol is not supported at design time at the moment.
                }
            }

            // Split aspect instances by syntax tree.
            foreach ( var aspectInstance in pipelineResults.AspectInstances )
            {
                var targetDeclarationId = aspectInstance.TargetDeclaration.ToSerializableId();

                var syntaxTree = aspectInstance.TargetDeclaration.GetPrimarySyntaxTree( compilation.CompilationContext );

                if ( syntaxTree == null )
                {
                    // Skipping because we don't have a syntax tree.
                    continue;
                }

                var filePath = syntaxTree.FilePath;
                var builder = resultBuilders[filePath];
                builder.AspectInstances ??= ImmutableArray.CreateBuilder<DesignTimeAspectInstance>();

                builder.AspectInstances.Add(
                    new DesignTimeAspectInstance(
                        targetDeclarationId,
                        aspectInstance.AspectClass.FullName ) );
            }

            // Split transformations by syntax tree.
            foreach ( var transformation in pipelineResults.Transformations )
            {
                var targetSymbol = transformation.TargetDeclaration.GetSymbol();

                if ( targetSymbol == null )
                {
                    // Transformations on introduced declarations are not represented at design time at the moment.
                    continue;
                }

                var syntaxTree = targetSymbol.GetPrimarySyntaxReference().AssertNotNull().SyntaxTree;
                var filePath = syntaxTree.FilePath;
                var builder = resultBuilders[filePath];
                builder.Transformations ??= ImmutableArray.CreateBuilder<DesignTimeTransformation>();

                builder.Transformations.Add(
                    new DesignTimeTransformation( transformation.TargetDeclaration.ToSerializableId(), transformation.AspectClass.FullName ) );
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

            // Return an immutable copy.
            return resultBuilders.SelectAsEnumerable( b => b.Value.ToImmutable( compilation.Compilation ) );
        }

        public Invalidator ToInvalidator() => new( this );

        internal (ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<CacheableScopedSuppression> Suppressions) GetDiagnosticsOnSyntaxTree( string path )
        {
            if ( this.SyntaxTreeResults.TryGetValue( path, out var syntaxTreeResult ) )
            {
                return (syntaxTreeResult.Diagnostics, syntaxTreeResult.Suppressions);
            }
            else
            {
                return (ImmutableArray<Diagnostic>.Empty, ImmutableArray<CacheableScopedSuppression>.Empty);
            }
        }

        public bool IsSyntaxTreeDirty( SyntaxTree syntaxTree ) => !this.SyntaxTreeResults.ContainsKey( syntaxTree.FilePath );

        public IEnumerable<string> InheritableAspectTypes => this._inheritableAspects.Keys;

        public IEnumerable<InheritableAspectInstance> GetInheritableAspects( string aspectType ) => this._inheritableAspects[aspectType];

        // The design-time implementation of validators does not use this property but GetValidatorsForSymbol.
        // (and cross-project design-time validators are not implemented)
        ImmutableArray<TransitiveValidatorInstance> ITransitiveAspectsManifest.Validators => ImmutableArray<TransitiveValidatorInstance>.Empty;

        public byte[] GetSerializedTransitiveAspectManifest( ProjectServiceProvider serviceProvider, Compilation compilation )
        {
            if ( this._serializedTransitiveAspectManifest == null )
            {
                var manifest = TransitiveAspectsManifest.Create(
                    this._inheritableAspects.SelectMany( g => g ).ToImmutableArray(),
                    this.ReferenceValidators.ToTransitiveValidatorInstances() );

                this._serializedTransitiveAspectManifest = manifest.ToBytes( serviceProvider, compilation );
            }

            return this._serializedTransitiveAspectManifest;
        }
    }
}