// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
    /// <summary>
    /// Computes and stores the changes between the last <see cref="Microsoft.CodeAnalysis.Compilation"/> and a new one.
    /// </summary>
    internal readonly struct CompilationVersion : ICompilationVersion
    {
        private readonly DiffStrategy _strategy;

        public ImmutableDictionary<string, SyntaxTreeVersion>? SyntaxTrees { get; }

        public ImmutableDictionary<AssemblyIdentity, CompilationReference>? References { get; }

        /// <summary>
        /// Gets the last <see cref="Microsoft.CodeAnalysis.Compilation"/>, or <c>null</c> if the <see cref="Update(Microsoft.CodeAnalysis.Compilation)"/> method
        /// has not been invoked yet.
        /// </summary>
        public Compilation? Compilation { get; }

        public CompilationChanges Changes { get; }

        public CompilationVersion( DiffStrategy strategy )
        {
            this._strategy = strategy;
            this.SyntaxTrees = default;
            this.Compilation = default;
            this.Changes = default;
            this.References = default;
        }

        public static CompilationVersion Create( Compilation compilation, DiffStrategy strategy, CancellationToken cancellationToken = default )
        {
            var compilationVersion = new CompilationVersion( strategy );

            return compilationVersion.Update( compilation, DependencyChanges.Empty, cancellationToken ).ResetChanges();
        }

        private CompilationVersion(
            Compilation? compilation,
            ImmutableDictionary<string, SyntaxTreeVersion>? syntaxTrees,
            ImmutableDictionary<AssemblyIdentity, CompilationReference>? references,
            CompilationChanges changes,
            DiffStrategy strategy )
        {
            this.SyntaxTrees = syntaxTrees;
            this.References = references;
            this.Compilation = compilation;
            this.Changes = changes;
            this._strategy = strategy;
        }

        public CompilationVersion ResetChanges()
        {
            if ( this.Compilation == null )
            {
                throw new InvalidOperationException();
            }

            return this.WithChanges( CompilationChanges.Empty( this.Compilation ) );
        }

        public CompilationVersion WithChanges( CompilationChanges changes )
        {
            if ( this.Compilation == null )
            {
                throw new InvalidOperationException();
            }

            return new CompilationVersion( this.Compilation, this.SyntaxTrees, this.References, changes, this._strategy );
        }

        private bool AreMetadataReferencesEqual( Compilation newCompilation )
        {
            // Detect changes in project references. 
            if ( this.Compilation == null )
            {
                return false;
            }

            var oldExternalReferences = this.Compilation.ExternalReferences;

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

        // Used for tests.
        internal CompilationVersion Update( Compilation newCompilation, CancellationToken cancellationToken = default )
            => this.Update( newCompilation, DependencyChanges.Empty, cancellationToken );

        /// <summary>
        /// Updates the <see cref="Compilation"/> property and returns the set of changes between the
        /// old value of <see cref="Compilation"/> and the newly provided <see cref="Microsoft.CodeAnalysis.Compilation"/>.
        /// </summary>
        public CompilationVersion Update(
            Compilation newCompilation,
            in DependencyChanges dependencyChanges,
            CancellationToken cancellationToken = default )
        {
            if ( newCompilation == this.Compilation )
            {
                return this;
            }

            var newTrees = ImmutableDictionary.CreateBuilder<string, SyntaxTreeVersion>( StringComparer.Ordinal );
            var generatedTrees = new List<SyntaxTree>();

            var syntaxTreeChanges = ImmutableDictionary.CreateBuilder<string, SyntaxTreeChange>( StringComparer.Ordinal );
            var addedPartialTypes = ImmutableHashSet<TypeDependencyKey>.Empty;

            var hasCompileTimeChange = dependencyChanges.HasCompileTimeChange || !this.AreMetadataReferencesEqual( newCompilation );

            // Process new trees.
            var lastTrees = this.SyntaxTrees;

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
                    if ( this._strategy.IsDifferent(
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

                        addedPartialTypes = addedPartialTypes.Union( change.AddedPartialTypes );
                        hasCompileTimeChange |= newSyntaxTreeVersion.HasCompileTimeCode || oldSyntaxTreeVersion.HasCompileTimeCode;
                    }
                }
                else
                {
                    // This is a new tree.
                    newSyntaxTreeVersion = this._strategy.GetSyntaxTreeVersion( newSyntaxTree, newCompilation );

                    compileTimeChangeKind = DiffStrategy.GetCompileTimeChangeKind( false, newSyntaxTreeVersion.HasCompileTimeCode );

                    var change = new SyntaxTreeChange(
                        newSyntaxTree.FilePath,
                        SyntaxTreeChangeKind.Added,
                        compileTimeChangeKind,
                        default,
                        newSyntaxTreeVersion );

                    syntaxTreeChanges.Add( newSyntaxTree.FilePath, change );

                    addedPartialTypes = addedPartialTypes.Union( change.AddedPartialTypes );
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
                    // The tree in itself has not changed, but a dependency has.

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

            // Determine which compilation should be analyzed.
            CompilationChanges compilationChanges;

            if ( !hasCompileTimeChange && syntaxTreeChanges.Count == 0 )
            {
                // There is no change, so we can analyze the previous compilation.
                compilationChanges = CompilationChanges.Empty( this.Compilation! );
            }
            else
            {
                // We have to analyze a new compilation, however we need to remove generated trees.
                cancellationToken.ThrowIfCancellationRequested();
                var compilationToAnalyze = newCompilation.RemoveSyntaxTrees( generatedTrees );

                compilationChanges = new CompilationChanges(
                    syntaxTreeChanges.ToImmutable(),
                    addedPartialTypes,
                    hasCompileTimeChange,
                    compilationToAnalyze,
                    this.Compilation != null );
            }

            compilationChanges = this.Changes.Merge( compilationChanges );

            // Process references.
            ImmutableDictionary<AssemblyIdentity, CompilationReference> references;

            if ( this.Compilation?.ExternalReferences == newCompilation.ExternalReferences )
            {
                references = this.References.AssertNotNull();
            }
            else
            {
                references = newCompilation.ExternalReferences.OfType<CompilationReference>()
                    .ToImmutableDictionary( r => r.Compilation.Assembly.Identity, r => r );
            }

            return new CompilationVersion( compilationChanges.CompilationToAnalyze, newTrees.ToImmutable(), references, compilationChanges, this._strategy );
        }

        AssemblyIdentity ICompilationVersion.AssemblyIdentity => this.Compilation.AssertNotNull().Assembly.Identity;

        ulong ICompilationVersion.CompileTimeProjectHash => throw new NotImplementedException();

        public bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion )
        {
            return this.SyntaxTrees.AssertNotNull().TryGetValue( path, out syntaxTreeVersion );
        }
    }
}