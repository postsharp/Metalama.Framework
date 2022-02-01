// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// Caches the pipeline results for each syntax tree.
    /// </summary>
    internal sealed class CompilationPipelineResult : ITransitiveAspectsManifest
    {
        private static readonly ImmutableDictionary<string, SyntaxTreePipelineResult> _emptySyntaxTreeResults =
            ImmutableDictionary.Create<string, SyntaxTreePipelineResult>( StringComparer.Ordinal );

        private static readonly ImmutableDictionary<string, IntroducedSyntaxTree> _emptyIntroducedSyntaxTrees =
            ImmutableDictionary.Create<string, IntroducedSyntaxTree>( StringComparer.Ordinal );

        private static readonly ImmutableDictionaryOfHashSet<string, InheritableAspectInstance> _emptyInheritableAspects =
            ImmutableDictionaryOfHashSet<string, InheritableAspectInstance>.Create(
                StringComparer.Ordinal,
                InheritableAspectInstance.ByTargetComparer.Instance );

        internal DesignTimeValidatorCollection Validators { get; } = DesignTimeValidatorCollection.Empty;

        public bool IsDirty { get; } = true;

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

        private CompilationPipelineResult(
            ImmutableDictionary<string, SyntaxTreePipelineResult> syntaxTreeResults,
            ImmutableDictionary<string, SyntaxTreePipelineResult> invalidSyntaxTreeResults,
            ImmutableDictionary<string, IntroducedSyntaxTree> introducedSyntaxTrees,
            ImmutableDictionaryOfHashSet<string, InheritableAspectInstance> inheritableAspects,
            DesignTimeValidatorCollection validators,
            bool isDirty )
        {
            this.SyntaxTreeResults = syntaxTreeResults;
            this._invalidSyntaxTreeResults = invalidSyntaxTreeResults;
            this.IntroducedSyntaxTrees = introducedSyntaxTrees;
            this._inheritableAspects = inheritableAspects;
            this.Validators = validators;
            this.IsDirty = isDirty;
        }

        internal CompilationPipelineResult() { }

        /// <summary>
        /// Updates cache with a <see cref="DesignTimePipelineExecutionResult"/> that includes results for several syntax trees.
        /// </summary>
        internal CompilationPipelineResult Update( PartialCompilation compilation, DesignTimePipelineExecutionResult pipelineResults )
        {
            var resultsByTree = SplitResultsByTree( compilation, pipelineResults );

            var syntaxTreeResultBuilder = this.SyntaxTreeResults.ToBuilder();

            ImmutableDictionary<string, IntroducedSyntaxTree>.Builder? introducedSyntaxTreeBuilder = null;
            ImmutableDictionaryOfHashSet<string, InheritableAspectInstance>.Builder? inheritableAspectsBuilder = null;
            DesignTimeValidatorCollection.Builder? validatorsBuilder = null;

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
                            introducedSyntaxTreeBuilder.Remove( introducedTree.Name );
                        }
                    }

                    if ( !oldSyntaxTreeResult.InheritableAspects.IsEmpty )
                    {
                        inheritableAspectsBuilder ??= this._inheritableAspects.ToBuilder();

                        foreach ( var x in oldSyntaxTreeResult.InheritableAspects )
                        {
                            inheritableAspectsBuilder.Remove( x.AspectType, x.AspectInstance );
                        }
                    }

                    if ( !oldSyntaxTreeResult.Validators.IsEmpty )
                    {
                        validatorsBuilder ??= this.Validators.ToBuilder();

                        foreach ( var validator in oldSyntaxTreeResult.Validators )
                        {
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
                        introducedSyntaxTreeBuilder.Add( introducedTree.Name, introducedTree );
                    }
                }

                if ( !result.InheritableAspects.IsEmpty )
                {
                    inheritableAspectsBuilder ??= this._inheritableAspects.ToBuilder();

                    foreach ( var x in result.InheritableAspects )
                    {
                        inheritableAspectsBuilder.Add( x.AspectType, x.AspectInstance );
                    }
                }

                if ( !result.Validators.IsDefaultOrEmpty )
                {
                    validatorsBuilder ??= this.Validators.ToBuilder();

                    foreach ( var validator in result.Validators )
                    {
                        validatorsBuilder.Add( validator );
                    }
                }

                syntaxTreeResultBuilder[filePath] = result;
            }

            var introducedTrees = introducedSyntaxTreeBuilder?.ToImmutable() ?? this.IntroducedSyntaxTrees;
            var inheritableAspects = inheritableAspectsBuilder?.ToImmutable() ?? this._inheritableAspects;
            var validators = validatorsBuilder?.ToImmutable() ?? this.Validators;

            return new CompilationPipelineResult(
                syntaxTreeResultBuilder.ToImmutable(),
                ImmutableDictionary<string, SyntaxTreePipelineResult>.Empty,
                introducedTrees,
                inheritableAspects,
                validators,
                false );
        }

        /// <summary>
        /// Splits a <see cref="DesignTimePipelineExecutionResult"/>, which includes data for several syntax trees, into
        /// a list of <see cref="SyntaxTreePipelineResult"/> which each have information related to a single syntax tree.
        /// </summary>
        /// <param name="pipelineResults"></param>
        /// <returns></returns>
        private static IEnumerable<SyntaxTreePipelineResult> SplitResultsByTree(
            PartialCompilation compilation,
            DesignTimePipelineExecutionResult pipelineResults )
        {
            var resultBuilders = pipelineResults
                .InputSyntaxTrees
                .ToDictionary( r => r.Key, syntaxTree => new SyntaxTreePipelineResultBuilder( syntaxTree.Value ) );

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
                        var builder = resultBuilders[path!];
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
                var targetSymbol = inheritableAspectInstance.TargetDeclaration.GetSymbol( compilation.Compilation ).AssertNotNull();
                var syntaxTree = targetSymbol.GetPrimarySyntaxReference().AssertNotNull().SyntaxTree;
                var filePath = syntaxTree.FilePath;
                var builder = resultBuilders[filePath];
                builder.InheritableAspects ??= ImmutableArray.CreateBuilder<(string, InheritableAspectInstance)>();
                builder.InheritableAspects.Add( (inheritableAspectInstance.Aspect.GetType().FullName, inheritableAspectInstance) );
            }

            // Split validators by syntax tree.
            foreach ( var validator in pipelineResults.Validators )
            {
                var targetSymbol = validator.ValidatedDeclaration.GetSymbol().AssertNotNull();
                var syntaxTree = targetSymbol.GetPrimarySyntaxReference().AssertNotNull().SyntaxTree;
                var filePath = syntaxTree.FilePath;
                var builder = resultBuilders[filePath];
                builder.Validators ??= ImmutableArray.CreateBuilder<DesignTimeValidatorInstance>();

                builder.Validators.Add(
                    new DesignTimeValidatorInstance(
                        validator.ValidatedDeclaration.GetSymbol().AssertNotNull(),
                        validator.ReferenceKinds,
                        validator.Driver,
                        validator.Implementation ) );
            }

            // Add syntax trees with empty output to it gets cached too.
            var inputTreesWithoutOutput = compilation.SyntaxTrees.ToBuilder();

            foreach ( var path in resultBuilders.Keys )
            {
                inputTreesWithoutOutput.Remove( path );
            }

            foreach ( var empty in inputTreesWithoutOutput )
            {
                resultBuilders.Add( empty.Key, new SyntaxTreePipelineResultBuilder( empty.Value ) );
            }

            // Return an immutable copy.
            return resultBuilders.Select( b => b.Value.ToImmutable( compilation.Compilation ) );
        }

        internal CompilationPipelineResult Invalidate( CompilationChanges compilationChanges )
        {
            if ( !compilationChanges.HasChange )
            {
                // Nothing to do.
                return this;
            }
            else if ( compilationChanges.HasCompileTimeCodeChange )
            {
                return this.Clear();
            }
            else
            {
                var syntaxTreeBuilders = this.SyntaxTreeResults.ToBuilder();
                var invalidSyntaxTreeBuilders = this._invalidSyntaxTreeResults.ToBuilder();

                foreach ( var change in compilationChanges.SyntaxTreeChanges )
                {
                    switch ( change.SyntaxTreeChangeKind )
                    {
                        case SyntaxTreeChangeKind.Added:
                            break;

                        case SyntaxTreeChangeKind.Deleted:
                        case SyntaxTreeChangeKind.Changed:
                            Logger.DesignTime.Trace?.Log( $"DesignTimeSyntaxTreeResultCache.InvalidateCache({change.FilePath}): removed from cache." );

                            if ( syntaxTreeBuilders.TryGetValue( change.FilePath, out var oldSyntaxTreeResult ) )
                            {
                                syntaxTreeBuilders.Remove( change.FilePath );
                                invalidSyntaxTreeBuilders.Add( change.FilePath, oldSyntaxTreeResult );
                            }

                            break;
                    }
                }

                return new CompilationPipelineResult(
                    syntaxTreeBuilders.ToImmutable(),
                    invalidSyntaxTreeBuilders.ToImmutable(),
                    this.IntroducedSyntaxTrees,
                    this._inheritableAspects,
                    this.Validators,
                    true );
            }
        }

#pragma warning disable CA1822
        public CompilationPipelineResult Clear()
#pragma warning restore CA1822
        {
            Logger.DesignTime.Trace?.Log( $"DesignTimeSyntaxTreeResultCache.Clear()." );

            return new CompilationPipelineResult();
        }

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

        public IEnumerable<InheritableAspectInstance> GetInheritedAspects( string aspectType ) => this._inheritableAspects[aspectType];

        // The design-time implementation of validators does not use this property but GetValidatorsForSymbol.
        // (and cross-project design-time validators are not implemented)
        ImmutableArray<TransitiveValidatorInstance> ITransitiveAspectsManifest.Validators => ImmutableArray<TransitiveValidatorInstance>.Empty;
    }
}