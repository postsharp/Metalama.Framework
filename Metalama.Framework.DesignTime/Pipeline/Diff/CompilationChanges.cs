// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
    /// <summary>
    /// Represents changes between two instances of the <see cref="Microsoft.CodeAnalysis.Compilation"/> class.
    /// </summary>
    internal class CompilationChanges
    {
        private readonly ImmutableDictionary<string, SyntaxTreeChange> _syntaxTreeChanges;
        private readonly DiffStrategy _strategy;

        public CompilationVersion? OldCompilationVersion { get; }

        public CompilationVersion NewCompilationVersion { get; }

        /// <summary>
        /// Gets the set of syntax tree changes.
        /// </summary>
        public IEnumerable<SyntaxTreeChange> SyntaxTreeChanges => this._syntaxTreeChanges.Values;

        /// <summary>
        /// Gets a value indicating whether the changes affects the compile-time subproject.
        /// </summary>
        public bool HasCompileTimeCodeChange { get; }

        private CompilationChanges(
            DiffStrategy strategy,
            CompilationVersion? oldCompilationVersion,
            CompilationVersion newCompilationVersion,
            ImmutableDictionary<string, SyntaxTreeChange> syntaxTreeChanges,
            bool hasCompileTimeCodeChange,
            Compilation compilationToAnalyze,
            bool isIncremental )
        {
            this._strategy = strategy;
            this._syntaxTreeChanges = syntaxTreeChanges;
            this.OldCompilationVersion = oldCompilationVersion;
            this.NewCompilationVersion = newCompilationVersion;
            this.HasCompileTimeCodeChange = hasCompileTimeCodeChange;
            this.CompilationToAnalyze = compilationToAnalyze;
            this.IsIncremental = isIncremental;
        }

        /// <summary>
        /// Gets a <see cref="CompilationChanges"/> object that represents the absence of change.
        /// </summary>
        public static CompilationChanges Empty( CompilationVersion? oldCompilation, CompilationVersion newCompilation )
            => new(
                newCompilation.Strategy,
                oldCompilation,
                newCompilation,
                ImmutableDictionary<string, SyntaxTreeChange>.Empty,
                false,
                newCompilation.Compilation,
                oldCompilation != null );

        public bool HasChange => this._syntaxTreeChanges is { Count: > 0 } || this.HasCompileTimeCodeChange;

        public bool IsIncremental { get; }

        /// <summary>
        /// Gets the <see cref="Microsoft.CodeAnalysis.Compilation"/> that must be analyzed. If <see cref="HasChange"/> is false,
        /// this is the last compilation of <see cref="CompilationVersion"/>. Otherwise, this is the new compilation. 
        /// </summary>
        public Compilation CompilationToAnalyze { get; }

        public CompilationChanges Merge( CompilationChanges compilationChanges )
        {
            if ( !this.HasChange || !compilationChanges.IsIncremental )
            {
                return compilationChanges;
            }
            else if ( !compilationChanges.HasChange )
            {
                return this;
            }
            else
            {
                // Merge syntax tree changes.
                var mergedSyntaxTreeBuilder = this._syntaxTreeChanges.ToBuilder();

                foreach ( var syntaxTreeChanges in compilationChanges._syntaxTreeChanges )
                {
                    if ( !mergedSyntaxTreeBuilder.TryGetValue( syntaxTreeChanges.Key, out var oldSyntaxTreeChange ) )
                    {
                        mergedSyntaxTreeBuilder.Add( syntaxTreeChanges );
                    }
                    else
                    {
                        var merged = oldSyntaxTreeChange.Merge( syntaxTreeChanges.Value );

                        if ( merged.SyntaxTreeChangeKind == SyntaxTreeChangeKind.None )
                        {
                            mergedSyntaxTreeBuilder.Remove( syntaxTreeChanges.Key );
                        }
                        else
                        {
                            mergedSyntaxTreeBuilder[syntaxTreeChanges.Key] = merged;
                        }
                    }
                }

                return new CompilationChanges(
                    this._strategy,
                    this.OldCompilationVersion,
                    compilationChanges.NewCompilationVersion,
                    mergedSyntaxTreeBuilder.ToImmutable(),
                    this.HasCompileTimeCodeChange | compilationChanges.HasCompileTimeCodeChange,
                    compilationChanges.CompilationToAnalyze,
                    this.IsIncremental );
            }
        }

        public static CompilationChanges NonIncremental( CompilationVersion compilationVersion )
        {
            var syntaxTreeChanges = compilationVersion.SyntaxTrees.ToImmutableDictionary( t => t.Key, t => SyntaxTreeChange.NonIncremental( t.Value ) );

            return new CompilationChanges(
                compilationVersion.Strategy,
                null,
                compilationVersion,
                syntaxTreeChanges,
                true,
                compilationVersion.Compilation,
                false );
        }

        public static CompilationChanges Incremental(
            CompilationVersion oldCompilationVersion,
            Compilation newCompilation,
            DependencyChanges dependencyChanges = default,
            CancellationToken cancellationToken = default )
        {
            if ( newCompilation == oldCompilationVersion.Compilation )
            {
                return Empty( oldCompilationVersion, oldCompilationVersion.WithCompilation( newCompilation ) );
            }

            if ( dependencyChanges.IsUninitialized )
            {
                dependencyChanges = DependencyChanges.Empty;
            }

            oldCompilationVersion.Strategy.Observer?.OnUpdateCompilationVersion();

            var newTrees = ImmutableDictionary.CreateBuilder<string, SyntaxTreeVersion>( StringComparer.Ordinal );
            var generatedTrees = new List<SyntaxTree>();

            var syntaxTreeChanges = ImmutableDictionary.CreateBuilder<string, SyntaxTreeChange>( StringComparer.Ordinal );

            var hasCompileTimeChange = dependencyChanges.HasCompileTimeChange
                                       || !AreMetadataReferencesEqual( oldCompilationVersion.Compilation, newCompilation );

            // Process new trees.
            var lastTrees = oldCompilationVersion.SyntaxTrees;

            foreach ( var newSyntaxTree in newCompilation.SyntaxTrees )
            {
                cancellationToken.ThrowIfCancellationRequested();

                CompileTimeChangeKind compileTimeChangeKind;

                // Files generated by us are ignored during the comparison.
                if ( SourceGeneratorHelper.IsGeneratedFile( newSyntaxTree ) )
                {
                    generatedTrees.Add( newSyntaxTree );

                    continue;
                }

                // At design time, the collection of syntax trees can contain duplicates.
                if ( newTrees.TryGetValue( newSyntaxTree.FilePath, out var existingNewTree ) )
                {
                    if ( existingNewTree.SyntaxTree != newSyntaxTree )
                    {
                        throw new AssertionFailedException();
                    }

                    continue;
                }

                SyntaxTreeVersion newSyntaxTreeVersion;

                if ( lastTrees != null && lastTrees.TryGetValue( newSyntaxTree.FilePath, out var oldSyntaxTreeVersion ) )
                {
                    if ( oldCompilationVersion.Strategy.IsDifferent(
                            oldSyntaxTreeVersion,
                            newSyntaxTree,
                            newCompilation,
                            out newSyntaxTreeVersion ) )
                    {
                        compileTimeChangeKind = DiffStrategy.GetCompileTimeChangeKind(
                            oldSyntaxTreeVersion.HasCompileTimeCode,
                            newSyntaxTreeVersion.HasCompileTimeCode );

                        var change = new SyntaxTreeChange(
                            newSyntaxTree.FilePath,
                            SyntaxTreeChangeKind.Changed,
                            compileTimeChangeKind,
                            oldSyntaxTreeVersion,
                            newSyntaxTreeVersion );

                        syntaxTreeChanges.Add( newSyntaxTree.FilePath, change );

                        hasCompileTimeChange |= newSyntaxTreeVersion.HasCompileTimeCode || oldSyntaxTreeVersion.HasCompileTimeCode;
                    }
                }
                else
                {
                    // This is a new tree.
                    newSyntaxTreeVersion = oldCompilationVersion.Strategy.GetSyntaxTreeVersion( newSyntaxTree, newCompilation );

                    compileTimeChangeKind = DiffStrategy.GetCompileTimeChangeKind( false, newSyntaxTreeVersion.HasCompileTimeCode );

                    var change = new SyntaxTreeChange(
                        newSyntaxTree.FilePath,
                        SyntaxTreeChangeKind.Added,
                        compileTimeChangeKind,
                        default,
                        newSyntaxTreeVersion );

                    syntaxTreeChanges.Add( newSyntaxTree.FilePath, change );

                    hasCompileTimeChange |= newSyntaxTreeVersion.HasCompileTimeCode;
                }

                newTrees.Add( newSyntaxTree.FilePath, newSyntaxTreeVersion );
                lastTrees = lastTrees?.Remove( newSyntaxTree.FilePath );
            }

            // Process old trees.
            if ( lastTrees != null )
            {
                foreach ( var oldSyntaxTree in lastTrees )
                {
                    syntaxTreeChanges.Add(
                        oldSyntaxTree.Key,
                        new SyntaxTreeChange(
                            oldSyntaxTree.Key,
                            SyntaxTreeChangeKind.Deleted,
                            DiffStrategy.GetCompileTimeChangeKind( oldSyntaxTree.Value.HasCompileTimeCode, false ),
                            oldSyntaxTree.Value,
                            default ) );
                }
            }

            // Process dependencies.
            foreach ( var dependency in dependencyChanges.InvalidatedSyntaxTrees )
            {
                if ( !syntaxTreeChanges.TryGetValue( dependency, out var change ) || change.SyntaxTreeChangeKind == SyntaxTreeChangeKind.None )
                {
                    // The tree in itself has not changed, but a dependency of the tree has.

                    if ( lastTrees == null || !lastTrees.TryGetValue( dependency, out var lastTree ) )
                    {
                        continue;
                    }

                    syntaxTreeChanges[dependency] = new SyntaxTreeChange(
                        dependency,
                        SyntaxTreeChangeKind.ChangedDependency,
                        CompileTimeChangeKind.None,
                        lastTree,
                        lastTree );
                }
            }

            // Process references.
            ImmutableDictionary<AssemblyIdentity, CompilationReference> references;

            if ( oldCompilationVersion.Compilation.ExternalReferences == newCompilation.ExternalReferences )
            {
                references = oldCompilationVersion.References.AssertNotNull();
            }
            else
            {
                references = newCompilation.ExternalReferences.OfType<CompilationReference>()
                    .ToImmutableDictionary( r => r.Compilation.Assembly.Identity, r => r );
            }

            // Create the new CompilationVersion.
            var syntaxTreeVersions = newTrees.ToImmutable();

            var newCompilationVersion = new CompilationVersion(
                oldCompilationVersion.Strategy,
                newCompilation,
                syntaxTreeVersions,
                references,
                DiffStrategy.ComputeCompileTimeProjectHash( syntaxTreeVersions ) );

            // Determine which compilation should be analyzed.
            CompilationChanges compilationChanges;

            if ( !hasCompileTimeChange && syntaxTreeChanges.Count == 0 )
            {
                // There is no significant change, so we can analyze the previous compilation.
                compilationChanges = Empty( oldCompilationVersion, oldCompilationVersion.WithCompilation( newCompilation ) );
            }
            else
            {
                // We have to analyze a new compilation, however we need to remove generated trees.
                cancellationToken.ThrowIfCancellationRequested();
                var compilationToAnalyze = newCompilation.RemoveSyntaxTrees( generatedTrees );

                compilationChanges = new CompilationChanges(
                    oldCompilationVersion.Strategy,
                    oldCompilationVersion,
                    newCompilationVersion,
                    syntaxTreeChanges.ToImmutable(),
                    hasCompileTimeChange,
                    compilationToAnalyze,
                    true );
            }

            return compilationChanges;
        }

        private static bool AreMetadataReferencesEqual( Compilation? oldCompilation, Compilation newCompilation )
        {
            // Detect changes in project references. 
            if ( oldCompilation == null )
            {
                return false;
            }

            var oldExternalReferences = oldCompilation.ExternalReferences;

            var newExternalReferences = newCompilation.ExternalReferences;

            Logger.DesignTime.Trace?.Log(
                $"Comparing metadata references: old count is {oldExternalReferences.Length}, new count is {newExternalReferences.Length}." );

            if ( oldExternalReferences == newExternalReferences )
            {
                return true;
            }

            // If the only differences are in compilation references, do not consider this as a difference.
            // Cross-project dependencies are not yet taken into consideration.
            var hasChange = false;

            if ( oldExternalReferences.Length != newExternalReferences.Length )
            {
                hasChange = true;
            }
            else
            {
                for ( var i = 0; i < oldExternalReferences.Length; i++ )
                {
                    if ( !MetadataReferencesEqual( oldExternalReferences[i], newExternalReferences[i] ) )
                    {
                        Logger.DesignTime.Trace?.Log( $"Difference found in {i}-th reference: '{oldExternalReferences[i]}' != '{newExternalReferences[i]}'." );
                        hasChange = true;

                        break;
                    }
                }
            }

            if ( hasChange )
            {
                Logger.DesignTime.Trace?.Log( "Change found in metadata reference. The last configuration cannot be reused." );

                return false;
            }

            return true;

            static bool MetadataReferencesEqual( MetadataReference a, MetadataReference b )
            {
                if ( a == b )
                {
                    return true;
                }
                else
                {
                    switch (a, b)
                    {
                        case (CompilationReference compilationReferenceA, CompilationReference compilationReferenceB):
                            // The way we compare in this case is naive, but we are processing cross-project dependencies through
                            // a different mechanism.
                            return compilationReferenceA.Compilation.AssemblyName == compilationReferenceB.Compilation.AssemblyName;

                        case (PortableExecutableReference portableExecutableReferenceA, PortableExecutableReference portableExecutableReferenceB):
                            return portableExecutableReferenceA.FilePath == portableExecutableReferenceB.FilePath;
                    }

                    return false;
                }
            }
        }

        public override string ToString() => $"HasCompileTimeCodeChange={this.HasCompileTimeCodeChange}, SyntaxTreeChanges={this._syntaxTreeChanges.Count}";
    }
}