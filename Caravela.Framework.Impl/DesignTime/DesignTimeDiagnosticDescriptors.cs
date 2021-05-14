// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using static Caravela.Framework.Diagnostics.Severity;

#pragma warning disable SA1118

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeDiagnosticDescriptors
    {
        // Reserved range 300-399

        private const string _category = "Caravela.DesignTime";

        internal static readonly DiagnosticDefinition<(ISymbol DeclaringSymbol, ISymbol ReferencedSymbol)>
            CannotReferenceCompileTimeOnly
                = new(
                    "CR0300",
                    "Cannot reference a compile-time-only declaration in a non-compile-time-only declaration.",
                    "Cannot reference '{1}' in '{0}' because '{1}' is compile-time-only but '{0}' is not. " +
                    "Consider adding [CompileTimeOnly] to '{0}', or do not use '{1}' in '{0}'.'",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserError
                = new(
                    "CR0301",
                    "A Caravela user error.",
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserWarning
                = new(
                    "CR0302",
                    "A Caravela user warning.",
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID.",
                    _category,
                    Warning );

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserInfo
                = new(
                    "CR0303",
                    "A Caravela user info.",
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID.",
                    _category,
                    Info );

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserHidden
                = new(
                    "CR0303",
                    "A Caravela hidden message.",
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID.",
                    _category,
                    Hidden );

        internal static readonly DiagnosticDefinition<ISymbol>
            CompileTimeTypeNeedsRebuild
                = new(
                    "CR0304",
                    "The compile-time type needs rebuild.",
                    "The compile-time type '{0}' has been modified since the last build. Caravela will stop analyzing this solution until the next build. "
                    + "To resume analysis, finish the work on all compile-time logic, and build the project (even if the run-time code still has issues).",
                    _category,
                    Error );
        
        internal static readonly DiagnosticDefinition<(string Id, ISymbol Symbol)>
            UnregisteredSuppression
                = new(
                    "CR0305",
                    "An aspect tried to suppress an unregistered diagnostic.",
                    "An aspect tried to suppress the diagnostic {0} on '{1}', but this diagnostic ID has not been configured for suppression in the user profile.",
                    _category,
                    Warning );
    }
}