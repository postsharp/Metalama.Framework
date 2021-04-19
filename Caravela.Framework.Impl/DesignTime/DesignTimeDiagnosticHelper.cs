// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeDiagnosticHelper
    {
        public static void ReportDiagnostics(
            IEnumerable<Diagnostic> diagnostics,
            Compilation compilation,
            Action<Diagnostic> reportDiagnostic,
            bool wrapUnknownDiagnostics,
            SyntaxTree? syntaxTree = null )
        {
            var selectedDiagnostics = diagnostics;

            if ( syntaxTree != null )
            {
                selectedDiagnostics = selectedDiagnostics.Where( d => d.Location.SourceTree.FilePath == syntaxTree.FilePath );
            }

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

                if ( designTimeDiagnostic.Location.SourceTree == null || compilation.ContainsSyntaxTree( designTimeDiagnostic.Location.SourceTree ) )
                {
                    reportDiagnostic( designTimeDiagnostic );
                }
                else
                {
                    var originalSyntaxTree =
                        compilation.SyntaxTrees.SingleOrDefault( t => t.FilePath == designTimeDiagnostic.Location.SourceTree.FilePath );

                    if ( originalSyntaxTree != null )
                    {
                        var newLocation = originalSyntaxTree.GetLocation( diagnostic.Location.SourceSpan );

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
        }
    }
}