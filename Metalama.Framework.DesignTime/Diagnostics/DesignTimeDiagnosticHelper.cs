// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.DesignTime.Diagnostics
{
    internal static class DesignTimeDiagnosticHelper
    {
        /// <summary>
        /// Reports a list diagnostics in a <see cref="SyntaxTree"/>. The diagnostics may have been produced for an older version of the <see cref="SyntaxTree"/>.
        /// In this case, the method computes the new location of the syntax tree. Diagnostics of unknown types are wrapped into well-known diagnostics.
        /// </summary>
        /// <param name="diagnostics">List of diagnostics to be reported.</param>
        /// <param name="compilation">The compilation in which diagnostics must be reported.</param>
        /// <param name="reportDiagnostic">The delegate to call to report a diagnostic.</param>
        /// <param name="wrapUnknownDiagnostics">Determines whether unknown diagnostics should be wrapped into known diagnostics.</param>
        public static void ReportDiagnostics(
            IEnumerable<Diagnostic> diagnostics,
            Compilation compilation,
            Action<Diagnostic> reportDiagnostic,
            bool wrapUnknownDiagnostics )
        {
            foreach ( var diagnostic in diagnostics )
            {
                Diagnostic designTimeDiagnostic;

                if ( !wrapUnknownDiagnostics || DesignTimeDiagnosticDefinitions.GetInstance().SupportedDiagnosticDescriptors.ContainsKey( diagnostic.Id ) )
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

                    designTimeDiagnostic = descriptor.CreateRoslynDiagnostic(
                        diagnostic.Location,
                        (diagnostic.Id, diagnostic.GetMessage()),
                        properties: diagnostic.Properties );
                }

                var reportSourceTree = designTimeDiagnostic.Location.SourceTree;

                if ( reportSourceTree == null || compilation.ContainsSyntaxTree( reportSourceTree ) )
                {
                    reportDiagnostic( designTimeDiagnostic );
                }
                else
                {
                    // Find the new syntax tree in the compilation.

                    // TODO: Optimize the indexation of the syntax trees of a compilation. We're doing that many times in many methods and we
                    // could have a weak dictionary mapping a compilation to an index of syntax trees.
                    var newSyntaxTree = compilation.SyntaxTrees.Single( t => t.FilePath == reportSourceTree.FilePath );

                    // Find the node in the new syntax tree corresponding to the node in the old syntax tree.
                    var oldNode = reportSourceTree.GetRoot().FindNode( diagnostic.Location.SourceSpan );

                    if ( !NodeFinder.TryFindOldNodeInNewTree( oldNode, newSyntaxTree, out var newNode ) )
                    {
                        // We could not find the old node in the new tree. This should not happen if cache invalidation is correct.
                        continue;
                    }

                    Location newLocation;

                    // Find the token in the new syntax tree corresponding to the token in the old syntax tree.
                    var oldToken = oldNode.FindToken( diagnostic.Location.SourceSpan.Start );
                    var newToken = newNode.ChildTokens().SingleOrDefault( t => t.Text == oldToken.Text );

                    if ( newToken.IsKind( SyntaxKind.None ) )
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
                            location: newLocation,
                            properties: designTimeDiagnostic.Properties );

                    reportDiagnostic( relocatedDiagnostic );
                }
            }
        }
    }
}