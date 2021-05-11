// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeDiagnosticDescriptors
    {
        // Reserved range 300-399

        private const string _category = "Caravela.DesignTime";

        internal static readonly StrongDiagnosticDescriptor<(ISymbol DeclaringSymbol, ISymbol ReferencedSymbol)>
            CannotReferenceCompileTimeOnly
                = new(
                    "CR0300",
                    "Cannot reference a compile-time-only declaration in a non-compile-time-only declaration.",
                    "Cannot reference '{1}' in '{0}' because '{1}' is compile-time-only but '{0}' is not. " +
                    "Consider adding [CompileTimeOnly] to '{0}', or do not use '{1}' in '{0}'.'",
                    _category,
                    DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<(string Id, string Message)>
            UserError
                = new(
                    "CR0301",
                    "A Caravela user error.",
                    "{0}: {1}",
                    _category,
                    DiagnosticSeverity.Error );

        internal static readonly StrongDiagnosticDescriptor<(string Id, string Message)>
            UserWarning
                = new(
                    "CR0302",
                    "A Caravela user warning.",
                    "{0}: {1}",
                    _category,
                    DiagnosticSeverity.Warning );

        internal static readonly StrongDiagnosticDescriptor<(string Id, string Message)>
            UserInfo
                = new(
                    "CR0303",
                    "A Caravela user info.",
                    "{0}: {1}",
                    _category,
                    DiagnosticSeverity.Info );

        internal static readonly StrongDiagnosticDescriptor<(string Id, string Message)>
            UserHidden
                = new(
                    "CR0303",
                    "A Caravela user info.",
                    "{0}: {1}",
                    _category,
                    DiagnosticSeverity.Hidden );
        
        internal static readonly StrongDiagnosticDescriptor<ISymbol>
            CompileTimeTypeNeedsRebuild
                = new(
                    "CR0304",
                    "The compile-time type needs rebuild.",
                    "The compile-time type '{0}' has been edited since the last build. Please build the project.",
                    _category,
                    DiagnosticSeverity.Error ); 
    }
}