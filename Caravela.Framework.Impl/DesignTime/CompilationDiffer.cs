// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class CompilationDiffer
    {
        private Dictionary<string, (SyntaxTree Tree, bool HasCompileTimeCode)>? _lastTrees;

        public Compilation? LastCompilation { get; private set; }

        public CompilationChanges GetChanges( Compilation newCompilation )
        {
            if ( newCompilation == this.LastCompilation )
            {
                return CompilationChanges.Empty;
            }

            lock ( this )
            {
                var newTrees = new Dictionary<string, (SyntaxTree Tree, bool HasCompileTimeCode)>( this._lastTrees?.Count ?? 32 );

                var syntaxTreeChanges = ImmutableArray.CreateBuilder<SyntaxTreeChange>();
                var hasCompileTimeChange = false;

                // Process new trees.
                foreach ( var newSyntaxTree in newCompilation.SyntaxTrees )
                {
                    bool hasCompileTimeCode;
                    CompileTimeChangeKind compileTimeChangeKind;

                    if ( this._lastTrees != null && this._lastTrees.TryGetValue( newSyntaxTree.FilePath, out var oldEntry ) )
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
                    this._lastTrees?.Remove( newSyntaxTree.FilePath );
                }

                // Process old trees.
                if ( this._lastTrees != null )
                {
                    foreach ( var oldSyntaxTree in this._lastTrees )
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

                // Swap.
                this._lastTrees = newTrees;
                this.LastCompilation = newCompilation;

                return new CompilationChanges( syntaxTreeChanges.ToImmutable(), hasCompileTimeChange );
            }
        }

        private static CompileTimeChangeKind GetCompileTimeChangeKind( bool oldValue, bool newValue )
            => (oldValue, newValue) switch
            {
                (true, true) => CompileTimeChangeKind.None,
                (false, false) => CompileTimeChangeKind.None,
                (true, false) => CompileTimeChangeKind.NoLongerCompileTime,
                (false, true) => CompileTimeChangeKind.NewlyCompileTime
            };

        internal static bool IsDifferent( SyntaxTree oldSyntaxTree, SyntaxTree newSyntaxTree ) => IsDifferent( oldSyntaxTree, newSyntaxTree, out _ );

        internal static bool IsDifferent( SyntaxTree oldSyntaxTree, SyntaxTree newSyntaxTree, out bool? hasCompileTimeCode )
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
                                    if ( changedTrivia.Span.Length == changedSpan.Length && (change.NewText == null || change.NewText.Length == 0) )
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
                => node.Parent switch
                {
                    BaseMethodDeclarationSyntax method => node == method.Body || node == method.ExpressionBody,
                    AccessorDeclarationSyntax accessor => node == accessor.Body || node == accessor.ExpressionBody,
                    VariableDeclaratorSyntax field => node == field.Initializer,
                    PropertyDeclarationSyntax property => node == property.ExpressionBody || node == property.Initializer,
                    _ => node.Parent != null && IsChangeIrrelevantToSymbol( node.Parent )
                };
        }

        public void Reset()
        {
            this.LastCompilation = null;
            this._lastTrees = null;
        }
    }
}