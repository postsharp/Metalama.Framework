// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
    /// <summary>
    /// Computes the changes between the last <see cref="Compilation"/> and a new one.
    /// </summary>
    internal readonly struct CompilationChangeTracker
    {
        private readonly ImmutableDictionary<string, (SyntaxTree Tree, bool HasCompileTimeCode, ulong Hash)>? _lastTrees;

        /// <summary>
        /// Gets the last <see cref="Compilation"/>, or <c>null</c> if the <see cref="Update"/> method
        /// has not been invoked yet.
        /// </summary>
        public Compilation? LastCompilation { get; }

        public CompilationChanges? UnprocessedChanges { get; }

        private CompilationChangeTracker(
            ImmutableDictionary<string, (SyntaxTree Tree, bool HasCompileTimeCode, ulong Hash)>? lastTrees,
            Compilation? lastCompilation,
            CompilationChanges? unprocessedChanges )
        {
            this._lastTrees = lastTrees;
            this.LastCompilation = lastCompilation;
            this.UnprocessedChanges = unprocessedChanges;
        }

        public CompilationChangeTracker ResetUnprocessedChanges()
        {
            if ( this.LastCompilation == null || this.UnprocessedChanges == null )
            {
                throw new InvalidOperationException();
            }

            return new CompilationChangeTracker( this._lastTrees, this.LastCompilation, CompilationChanges.Empty( this.LastCompilation ) );
        }

        private bool AreMetadataReferencesEqual( Compilation newCompilation )
        {
            // Detect changes in project references. 
            if ( this.LastCompilation == null )
            {
                return false;
            }

            var oldExternalReferences = this.LastCompilation.ExternalReferences;

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

        /// <summary>
        /// Updates the <see cref="LastCompilation"/> property and returns the set of changes between the
        /// old value of <see cref="LastCompilation"/> and the newly provided <see cref="Compilation"/>.
        /// </summary>
        public CompilationChangeTracker Update( Compilation newCompilation, CancellationToken cancellationToken )
        {
            if ( newCompilation == this.LastCompilation )
            {
                return this;
            }

            var areMetadataReferencesEqual = this.AreMetadataReferencesEqual( newCompilation );

            var newTrees = ImmutableDictionary.CreateBuilder<string, (SyntaxTree Tree, bool HasCompileTimeCode, ulong Hash)>( StringComparer.Ordinal );
            var generatedTrees = new List<SyntaxTree>();

            var syntaxTreeChanges = new List<SyntaxTreeChange>();
            var hasCompileTimeChange = !areMetadataReferencesEqual;

            // Process new trees.
            var lastTrees = this._lastTrees;

            foreach ( var newSyntaxTree in newCompilation.SyntaxTrees )
            {
                cancellationToken.ThrowIfCancellationRequested();

                CompileTimeChangeKind compileTimeChangeKind;

                // Generated files are ignored during the comparison.
                if ( SourceGeneratorHelper.IsGeneratedFile( newSyntaxTree ) )
                {
                    generatedTrees.Add( newSyntaxTree );

                    continue;
                }

                // At design time, the collection of syntax trees can contain duplicates.
                if ( newTrees.TryGetValue( newSyntaxTree.FilePath, out var existingNewTree ) )
                {
                    if ( existingNewTree.Tree != newSyntaxTree )
                    {
                        throw new AssertionFailedException();
                    }

                    continue;
                }

                bool newHasCompileTimeCode;

                ulong newSyntaxTreeHash;

                if ( lastTrees != null && lastTrees.TryGetValue( newSyntaxTree.FilePath, out var oldEntry ) )
                {
                    if ( IsDifferent(
                            oldEntry.Tree,
                            oldEntry.HasCompileTimeCode,
                            oldEntry.Hash,
                            newSyntaxTree,
                            out newHasCompileTimeCode,
                            out newSyntaxTreeHash ) )
                    {
                        compileTimeChangeKind = GetCompileTimeChangeKind( oldEntry.HasCompileTimeCode, newHasCompileTimeCode );

                        syntaxTreeChanges.Add(
                            new SyntaxTreeChange(
                                newSyntaxTree.FilePath,
                                SyntaxTreeChangeKind.Changed,
                                newHasCompileTimeCode,
                                compileTimeChangeKind,
                                newSyntaxTree,
                                oldEntry.Hash,
                                newSyntaxTreeHash ) );

                        hasCompileTimeChange |= newHasCompileTimeCode || oldEntry.HasCompileTimeCode;
                    }
                }
                else
                {
                    // This is a new tree.
                    AnalyzeSyntaxTree( newSyntaxTree, out newHasCompileTimeCode, out newSyntaxTreeHash );

                    compileTimeChangeKind = GetCompileTimeChangeKind( false, newHasCompileTimeCode );

                    syntaxTreeChanges.Add(
                        new SyntaxTreeChange(
                            newSyntaxTree.FilePath,
                            SyntaxTreeChangeKind.Added,
                            newHasCompileTimeCode,
                            compileTimeChangeKind,
                            newSyntaxTree,
                            0,
                            newSyntaxTreeHash ) );

                    hasCompileTimeChange |= newHasCompileTimeCode;
                }

                newTrees.Add( newSyntaxTree.FilePath, (newSyntaxTree, newHasCompileTimeCode, newSyntaxTreeHash) );
                lastTrees = lastTrees?.Remove( newSyntaxTree.FilePath );
            }

            // Process old trees.
            if ( lastTrees != null )
            {
                foreach ( var oldSyntaxTree in lastTrees )
                {
                    syntaxTreeChanges.Add(
                        new SyntaxTreeChange(
                            oldSyntaxTree.Key,
                            SyntaxTreeChangeKind.Deleted,
                            false,
                            GetCompileTimeChangeKind( oldSyntaxTree.Value.HasCompileTimeCode, false ),
                            null,
                            oldSyntaxTree.Value.Hash,
                            0 ) );
                }
            }

            // Determine which compilation should be analyzed.
            CompilationChanges compilationChanges;

            if ( !hasCompileTimeChange && syntaxTreeChanges.Count == 0 )
            {
                // There is no change, so we can analyze the previous compilation.
                compilationChanges = CompilationChanges.Empty( this.LastCompilation! );
            }
            else
            {
                // We have to analyze a new compilation, however we need to remove generated trees.
                cancellationToken.ThrowIfCancellationRequested();
                var compilationToAnalyze = newCompilation.RemoveSyntaxTrees( generatedTrees );

                compilationChanges = new CompilationChanges(
                    syntaxTreeChanges,
                    hasCompileTimeChange,
                    compilationToAnalyze,
                    this.LastCompilation != null );
            }

            if ( this.UnprocessedChanges != null )
            {
                compilationChanges = this.UnprocessedChanges.Merge( compilationChanges );
            }

            return new CompilationChangeTracker( newTrees.ToImmutable(), compilationChanges.CompilationToAnalyze, compilationChanges );
        }

        private static CompileTimeChangeKind GetCompileTimeChangeKind( bool oldValue, bool newValue )
            => (oldValue, newValue) switch
            {
                (true, true) => CompileTimeChangeKind.None,
                (false, false) => CompileTimeChangeKind.None,
                (true, false) => CompileTimeChangeKind.NoLongerCompileTime,
                (false, true) => CompileTimeChangeKind.NewlyCompileTime
            };

        /// <summary>
        /// Determines whether two syntax trees are significantly different. This overload is called from tests.
        /// </summary>
        internal static bool IsDifferent( SyntaxTree oldSyntaxTree, SyntaxTree newSyntaxTree )
        {
            AnalyzeSyntaxTree( oldSyntaxTree, out var oldHasCompileTimeCode, out var oldSyntaxTreeHash );

            return IsDifferent( oldSyntaxTree, oldHasCompileTimeCode, oldSyntaxTreeHash, newSyntaxTree, out _, out _ );
        }

        private static bool IsDifferent(
            SyntaxTree oldSyntaxTree,
            bool oldHasCompileTimeCode,
            ulong oldSyntaxTreeHash,
            SyntaxTree newSyntaxTree,
            out bool newHasCompileTimeCode,
            out ulong newSyntaxTreeHash )
        {
            // Check if the source text has changed.
            if ( newSyntaxTree == oldSyntaxTree )
            {
                newSyntaxTreeHash = oldSyntaxTreeHash;
                newHasCompileTimeCode = oldHasCompileTimeCode;

                return false;
            }
            else
            {
                var newSyntaxRoot = newSyntaxTree.GetRoot();
                newHasCompileTimeCode = CompileTimeCodeFastDetector.HasCompileTimeCode( newSyntaxRoot );
                var hhx64 = new XXH64();
                BaseCodeHasher hasher = newHasCompileTimeCode ? new CompileTimeCodeHasher( hhx64 ) : new RunTimeCodeHasher( hhx64 );
                hasher.Visit( newSyntaxRoot );
                newSyntaxTreeHash = hhx64.Digest();

                return newSyntaxTreeHash != oldSyntaxTreeHash;
            }
        }

        private static void AnalyzeSyntaxTree( SyntaxTree syntaxTree, out bool hasCompileTimeCode, out ulong hash )
        {
            var newSyntaxRoot = syntaxTree.GetRoot();
            hasCompileTimeCode = CompileTimeCodeFastDetector.HasCompileTimeCode( newSyntaxRoot );
            var hhx64 = new XXH64();
            BaseCodeHasher hasher = hasCompileTimeCode ? new CompileTimeCodeHasher( hhx64 ) : new RunTimeCodeHasher( hhx64 );
            hasher.Visit( newSyntaxRoot );
            hash = hhx64.Digest();
        }
    }
}