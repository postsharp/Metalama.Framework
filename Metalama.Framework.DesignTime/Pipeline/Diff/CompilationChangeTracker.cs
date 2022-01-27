// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
    /// <summary>
    /// Computes the changes between the last <see cref="Compilation"/> and a new one.
    /// </summary>
    internal readonly struct CompilationChangeTracker
    {
        private readonly ImmutableDictionary<string, (SyntaxTree Tree, bool HasCompileTimeCode)>? _lastTrees;

        /// <summary>
        /// Gets the last <see cref="Compilation"/>, or <c>null</c> if the <see cref="Update"/> method
        /// has not been invoked yet.
        /// </summary>
        public Compilation? LastCompilation { get; }

        public CompilationChanges? UnprocessedChanges { get; }

        private CompilationChangeTracker(
            ImmutableDictionary<string, (SyntaxTree Tree, bool HasCompileTimeCode)>? lastTrees,
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
                        hasChange = true;

                        break;
                    }
                }
            }

            if ( hasChange )
            {
                Logger.DesignTime.Trace?.Log( "Change found in metadata reference. The last configuration cannot be reused" );

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

            var newTrees = ImmutableDictionary.CreateBuilder<string, (SyntaxTree Tree, bool HasCompileTimeCode)>( StringComparer.Ordinal );
            var generatedTrees = new List<SyntaxTree>();

            var syntaxTreeChanges = new List<SyntaxTreeChange>();
            var hasCompileTimeChange = !areMetadataReferencesEqual;

            // Process new trees.
            var lastTrees = this._lastTrees;

            foreach ( var newSyntaxTree in newCompilation.SyntaxTrees )
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool hasCompileTimeCode;
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

                if ( lastTrees != null && lastTrees.TryGetValue( newSyntaxTree.FilePath, out var oldEntry ) )
                {
                    if ( IsDifferent( oldEntry.Tree, newSyntaxTree, out var newHasCompileTimeCode ) )
                    {
                        hasCompileTimeCode = newHasCompileTimeCode ?? CompileTimeCodeDetector.HasCompileTimeCode( newSyntaxTree.GetRoot() );
                        compileTimeChangeKind = GetCompileTimeChangeKind( oldEntry.HasCompileTimeCode, hasCompileTimeCode );

                        syntaxTreeChanges.Add(
                            new SyntaxTreeChange(
                                newSyntaxTree.FilePath,
                                SyntaxTreeChangeKind.Changed,
                                hasCompileTimeCode,
                                compileTimeChangeKind,
                                newSyntaxTree ) );

                        hasCompileTimeChange |= hasCompileTimeCode || oldEntry.HasCompileTimeCode;
                    }
                    else
                    {
                        hasCompileTimeCode = oldEntry.HasCompileTimeCode;
                    }
                }
                else
                {
                    // This is a new tree.

                    hasCompileTimeCode = CompileTimeCodeDetector.HasCompileTimeCode( newSyntaxTree.GetRoot() );
                    compileTimeChangeKind = GetCompileTimeChangeKind( false, hasCompileTimeCode );

                    syntaxTreeChanges.Add(
                        new SyntaxTreeChange(
                            newSyntaxTree.FilePath,
                            SyntaxTreeChangeKind.Added,
                            hasCompileTimeCode,
                            compileTimeChangeKind,
                            newSyntaxTree ) );

                    hasCompileTimeChange |= hasCompileTimeCode;
                }

                newTrees.Add( newSyntaxTree.FilePath, (newSyntaxTree, hasCompileTimeCode) );
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
                            null ) );
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
        internal static bool IsDifferent( SyntaxTree oldSyntaxTree, SyntaxTree newSyntaxTree ) => IsDifferent( oldSyntaxTree, newSyntaxTree, out _ );

        private static bool IsDifferent( SyntaxTree oldSyntaxTree, SyntaxTree newSyntaxTree, out bool? hasCompileTimeCode )
        {
            hasCompileTimeCode = null;

            // Check if the source text has changed.
            if ( newSyntaxTree == oldSyntaxTree )
            {
                return false;
            }
            else
            {
                if ( !newSyntaxTree.GetText().ContentEquals( oldSyntaxTree.GetText() ) )
                {
                    var oldSyntaxRoot = oldSyntaxTree.GetRoot();

                    // If the source text has changed, check whether the change can possibly change symbols. Changes in method implementations are ignored.
                    foreach ( var change in newSyntaxTree.GetChanges( oldSyntaxTree ) )
                    {
                        // change.Span is a span in the _old_ syntax tree.
                        // change.NewText is the new text.
                        var changedSpan = change.Span;

                        // If we are inserting a space, ignore it.
                        if ( changedSpan.Length == 0 && string.IsNullOrWhiteSpace( change.NewText ) )
                        {
                            continue;
                        }

                        // If we are editing a comment, ignore it.
                        var changedTrivia = oldSyntaxRoot.FindTrivia( changedSpan.Start );

                        var triviaKind = changedTrivia.Kind();

                        if ( triviaKind != SyntaxKind.None && changedTrivia.Span.Contains( changedSpan ) )
                        {
                            // If the change is totally contained in a trivia, excluding the trivia prefix, we may ignore it

                            switch ( triviaKind )
                            {
                                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                                case SyntaxKind.XmlComment:
                                    if ( changedSpan.Start > changedTrivia.Span.Start + 3 )
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        break;
                                    }

                                case SyntaxKind.MultiLineCommentTrivia:
                                case SyntaxKind.SingleLineCommentTrivia:
                                    if ( changedSpan.Start > changedTrivia.Span.Start + 3 )
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        break;
                                    }

                                case SyntaxKind.WhitespaceTrivia:
                                    if ( changedTrivia.Span.Length == changedSpan.Length && string.IsNullOrEmpty( change.NewText ) )
                                    {
                                        // Removing all spaces of a trivia is potentially breaking.
                                        break;
                                    }
                                    else if ( !string.IsNullOrWhiteSpace( change.NewText ) )
                                    {
                                        // Adding non-whitespace to a whitespace is breaking.
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                            }
                        }

                        // If the change is in a method body or other expression, ignore it.
                        hasCompileTimeCode = CompileTimeCodeDetector.HasCompileTimeCode( newSyntaxTree.GetRoot() );

                        if ( !hasCompileTimeCode.Value )
                        {
                            if ( !oldSyntaxRoot.FullSpan.Contains( changedSpan ) )
                            {
                                throw new AssertionFailedException();
                            }

                            var changedNode = oldSyntaxRoot.FindNode( changedSpan );

                            if ( IsChangeIrrelevantToSymbol( changedNode ) )
                            {
                                continue;
                            }
                        }

                        // If we are here, it means that we have a relevant change.
                        return true;
                    }
                }

                return false;
            }

            // Determines if a change in a node can possibly affect a change in symbols.
            static bool IsChangeIrrelevantToSymbol( SyntaxNode node )
            {
                return node.Parent switch
                {
                    BaseMethodDeclarationSyntax method => node == method.Body || node == method.ExpressionBody,
                    AccessorDeclarationSyntax accessor => node == accessor.Body || node == accessor.ExpressionBody,
                    VariableDeclaratorSyntax field => node == field.Initializer,
                    PropertyDeclarationSyntax property => node == property.ExpressionBody || node == property.Initializer,
                    _ => node.Parent != null && IsChangeIrrelevantToSymbol( node.Parent )
                };
            }
        }
    }
}