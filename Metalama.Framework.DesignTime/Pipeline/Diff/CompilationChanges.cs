﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
    /// <summary>
    /// Represents changes between two instances of the <see cref="Microsoft.CodeAnalysis.Compilation"/> class.
    /// </summary>
    internal class CompilationChanges
    {
        public ImmutableDictionary<string, SyntaxTreeChange> SyntaxTreeChanges { get; }

        public CompilationVersion? OldCompilationVersion { get; }

        public CompilationVersion NewCompilationVersion { get; }

        public ImmutableDictionary<AssemblyIdentity, ReferencedCompilationChange> ReferencedCompilationChanges { get; }

        public AssemblyIdentity AssemblyIdentity => this.NewCompilationVersion.Compilation.Assembly.Identity;

        /// <summary>
        /// Gets a value indicating whether the changes affects the compile-time subproject.
        /// </summary>
        public bool HasCompileTimeCodeChange { get; }

        public CompilationChanges(
            CompilationVersion? oldCompilationVersion,
            CompilationVersion newCompilationVersion,
            ImmutableDictionary<string, SyntaxTreeChange> syntaxTreeChanges,
            ImmutableDictionary<AssemblyIdentity, ReferencedCompilationChange> referencedCompilationChanges,
            bool hasCompileTimeCodeChange,
            bool isIncremental )
        {
            this.SyntaxTreeChanges = syntaxTreeChanges;
            this.OldCompilationVersion = oldCompilationVersion;
            this.NewCompilationVersion = newCompilationVersion;
            this.ReferencedCompilationChanges = referencedCompilationChanges;
            this.HasCompileTimeCodeChange = hasCompileTimeCodeChange;
            this.IsIncremental = isIncremental;
        }

        /// <summary>
        /// Gets a <see cref="CompilationChanges"/> object that represents the absence of change.
        /// </summary>
        public static CompilationChanges Empty( CompilationVersion? oldCompilation, CompilationVersion newCompilation )
            => new(
                oldCompilation,
                newCompilation,
                ImmutableDictionary<string, SyntaxTreeChange>.Empty,
                ImmutableDictionary<AssemblyIdentity, ReferencedCompilationChange>.Empty,
                false,
                oldCompilation != null );

        public bool HasChange => this.HasCompileTimeCodeChange || this.SyntaxTreeChanges.Count > 0 || this.ReferencedCompilationChanges.Count > 0;

        public bool IsIncremental { get; }

        public static CompilationChanges NonIncremental( CompilationVersion compilationVersion )
        {
            compilationVersion.Strategy.Observer?.OnComputeNonIncrementalChanges();

            var syntaxTreeChanges = compilationVersion.SyntaxTrees.ToImmutableDictionary( t => t.Key, t => SyntaxTreeChange.NonIncremental( t.Value ) );

            var references = compilationVersion.ReferencedCompilations.ToImmutableDictionary(
                x => x.Key,
                x => new ReferencedCompilationChange( null, x.Value.Compilation, ReferencedCompilationChangeKind.Added ) );

            return new CompilationChanges(
                null,
                compilationVersion,
                syntaxTreeChanges,
                references,
                true,
                false );
        }

        public static CompilationChanges Incremental(
            CompilationVersion oldCompilationVersion,
            Compilation newCompilation,
            ImmutableDictionary<AssemblyIdentity, ICompilationVersion> newReferences,
            ImmutableDictionary<AssemblyIdentity, ReferencedCompilationChange> referencedCompilationChanges,
            CancellationToken cancellationToken = default )
        {
            if ( newCompilation == oldCompilationVersion.Compilation )
            {
                return Empty( oldCompilationVersion, oldCompilationVersion.WithCompilation( newCompilation ) );
            }

            oldCompilationVersion.Strategy.Observer?.OnComputeIncrementalChanges();

            var newTrees = ImmutableDictionary.CreateBuilder<string, SyntaxTreeVersion>( StringComparer.Ordinal );
            var generatedTrees = new List<SyntaxTree>();

            var syntaxTreeChanges = ImmutableDictionary.CreateBuilder<string, SyntaxTreeChange>( StringComparer.Ordinal );

            var hasCompileTimeChange = referencedCompilationChanges.Any( c => c.Value.HasCompileTimeCodeChange );

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

            // Create the new CompilationVersion.
            var syntaxTreeVersions = newTrees.ToImmutable();

            // Determine which compilation should be analyzed.
            CompilationChanges compilationChanges;

            if ( !hasCompileTimeChange && syntaxTreeChanges.Count == 0 && referencedCompilationChanges.Count == 0 )
            {
                // There is no significant change, so we can analyze the previous compilation.
                compilationChanges = Empty( oldCompilationVersion, oldCompilationVersion.WithCompilation( newCompilation ) );
            }
            else
            {
                // We have to analyze a new compilation, however we need to remove generated trees.

                var compilationToAnalyze = newCompilation.RemoveSyntaxTrees( generatedTrees );

                var newCompilationVersion = new CompilationVersion(
                    oldCompilationVersion.Strategy,
                    newCompilation,
                    compilationToAnalyze,
                    syntaxTreeVersions,
                    newReferences,
                    DiffStrategy.ComputeCompileTimeProjectHash( syntaxTreeVersions ) );

                cancellationToken.ThrowIfCancellationRequested();

                compilationChanges = new CompilationChanges(
                    oldCompilationVersion,
                    newCompilationVersion,
                    syntaxTreeChanges.ToImmutable(),
                    referencedCompilationChanges,
                    hasCompileTimeChange,
                    true );
            }

            return compilationChanges;
        }

        public override string ToString() => $"HasCompileTimeCodeChange={this.HasCompileTimeCodeChange}, SyntaxTreeChanges={this.SyntaxTreeChanges.Count}";
    }
}