// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using static Metalama.Framework.Diagnostics.Severity;

#pragma warning disable SA1118

namespace Metalama.Framework.DesignTime
{
    internal static class DesignTimeDiagnosticDescriptors
    {
        // Reserved range 300-319

        private const string _category = "Metalama.DesignTime";

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserError
                = new(
                    "CR0301",
                    "A Metalama user error.",
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID.",
                    _category,
                    Error );

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserWarning
                = new(
                    "CR0302",
                    "A Metalama user warning.",
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID. "
                    + "Please restart your IDE.",
                    _category,
                    Warning );

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserInfo
                = new(
                    "CR0303",
                    "A Metalama user info.",
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID. "
                    + " Please restart your IDE.",
                    _category,
                    Info );

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserHidden
                = new(
                    "CR0304",
                    "A Metalama user hidden message.",
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID."
                    + " Please restart your IDE.",
                    _category,
                    Hidden );

        internal static readonly DiagnosticDefinition<(string Id, ISymbol Symbol)>
            UnregisteredSuppression
                = new(
                    "CR0306",
                    "An aspect tried to suppress an unregistered diagnostic.",
                    "An aspect tried to suppress the diagnostic {0} on '{1}', but this diagnostic ID has not been configured for "
                    + "suppression in the user profile. Please restart your IDE.",
                    _category,
                    Warning );
    }
}