// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeDiagnosticHelper
    {
        /// <summary>
        /// Reports a list diagnostics in a <see cref="SyntaxTree"/>. The diagnostics may have been produced for an older version of the <see cref="SyntaxTree"/>.
        /// In this case, the method computes the new location of the syntax tree. Diagnostics of unknown types are wrapped into well-known diagnostics.
        /// </summary>
        /// <param name="diagnostics">List of diagnostics to be reported.</param>
        /// <param name="newSyntaxTree">The current version of the syntax tree, on which diagnostics have to be reported.</param>
        /// <param name="reportDiagnostic">The delegate to call to report a diagnostic.</param>
        /// <param name="wrapUnknownDiagnostics">Determines whether unknown diagnostics should be wrapped into known diagnostics.</param>
        public static void ReportDiagnostics(
            IEnumerable<Diagnostic> diagnostics,
            SyntaxTree newSyntaxTree,
            Action<Diagnostic> reportDiagnostic,
            bool wrapUnknownDiagnostics )
        {
            var selectedDiagnostics = diagnostics.Where( d => d.Location.SourceTree?.FilePath == newSyntaxTree.FilePath );

            foreach ( var diagnostic in selectedDiagnostics )
            {
                Diagnostic designTimeDiagnostic;

                if ( !wrapUnknownDiagnostics || DesignTimeAnalyzer.DesignTimeDiagnosticIds.Contains( diagnostic.Id ) )
                {
                    designTimeDiagnostic = diagnostic;
                }
                else
                {
                    var descriptor =
                        diagnostic.Severity switch
                        {
                            DiagnosticSeverity.Error => DesignTimeDiagnosticDescriptors.UserError,
                            DiagnosticSeverity.Hidden => DesignTimeDiagnosticDescriptors.UserHidden,
                            DiagnosticSeverity.Warning => DesignTimeDiagnosticDescriptors.UserWarning,
                            DiagnosticSeverity.Info => DesignTimeDiagnosticDescriptors.UserInfo,
                            _ => throw new NotImplementedException()
                        };

                    designTimeDiagnostic = descriptor.CreateDiagnostic( diagnostic.Location, (diagnostic.Id, diagnostic.GetMessage()) );
                }

                var originalSourceTree = designTimeDiagnostic.Location.SourceTree;

                if ( originalSourceTree == null || originalSourceTree == newSyntaxTree )
                {
                    reportDiagnostic( designTimeDiagnostic );
                }
                else
                {
                    // Find the node in the new syntax tree corresponding to the node in the old syntax tree.
                    var oldNode = originalSourceTree.GetRoot().FindNode( diagnostic.Location.SourceSpan );

                    if ( !TryFindOldNodeInNewTree( oldNode, newSyntaxTree, out var newNode ) )
                    {
                        // We could not find the old node in the new tree. This should not happen if cache invalidation is correct.
                        continue;
                    }

                    Location newLocation;

                    // Find the token in the new syntax tree corresponding to the token in the old syntax tree.
                    var oldToken = oldNode.FindToken( diagnostic.Location.SourceSpan.Start );
                    var newToken = newNode.ChildTokens().SingleOrDefault( t => t.Text == oldToken.Text );

                    if ( newToken.Kind() == SyntaxKind.None )
                    {
                        // We could not find the old token in the new tree. This should not happen if cache invalidation is correct.
                        continue;
                    }

                    if ( newToken.Span.Length == diagnostic.Location.SourceSpan.Length )
                    {
                        // The diagnostic was reported to the exact token we found, so we can report it precisely.
                        newLocation = newToken.GetLocation();
                    }
                    else
                    {
                        // The diagnostic was reported on the syntax node we found, but not to an exact token. Report the
                        // diagnostic to the whole node instead.
                        newLocation = newNode.GetLocation();
                    }

                    var relocatedDiagnostic =
                        Diagnostic.Create(
                            designTimeDiagnostic.Id,
                            designTimeDiagnostic.Descriptor.Category,
                            new NonLocalizedString( diagnostic.GetMessage() ),
                            designTimeDiagnostic.Severity,
                            designTimeDiagnostic.DefaultSeverity,
                            true,
                            diagnostic.WarningLevel,
                            location: newLocation );

                    reportDiagnostic( relocatedDiagnostic );
                }
            }
        }

        private static bool TryFindOldNodeInNewTree( SyntaxNode oldNode, SyntaxTree newTree, out SyntaxNode newNode )
        {
            // Create a stack with the position of each ancestor node with respect to its parent.
            // We only position ourselves with respect to other nodes of the same kind, as we want to be less sensitive to changes in the new
            // syntax tree.

            Stack<(SyntaxKind Kind, int Position)> stack = new();

            for ( var oldNodeCursor = oldNode; oldNodeCursor?.Parent != null; oldNodeCursor = oldNodeCursor.Parent )
            {
                var syntaxKind = oldNodeCursor.Kind();
                var childrenOfSameKind = oldNodeCursor.Parent.ChildNodes().Where( n => n.Kind() == syntaxKind );

                var index = 0;

                foreach ( var childOfSameKind in childrenOfSameKind )
                {
                    if ( childOfSameKind == oldNodeCursor )
                    {
                        stack.Push( (syntaxKind, index) );
                        index = -1;

                        break;
                    }

                    index++;
                }

                if ( index != -1 )
                {
                    // We should have found ourselves in the parent node.
                    throw new AssertionFailedException();
                }
            }

            // Navigate the inverted stack from the new root.
            newNode = newTree.GetRoot();

            while ( stack.Count > 0 )
            {
                var slice = stack.Pop();
                var childrenOfSameKind = newNode.ChildNodes().Where( n => n.Kind() == slice.Kind );
                var childAtPosition = childrenOfSameKind.ElementAtOrDefault( slice.Position );

                if ( childAtPosition == null )
                {
                    return false;
                }
                else
                {
                    newNode = childAtPosition;
                }
            }

            return true;
        }
    }
}