// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using static Caravela.Framework.Diagnostics.Severity;

#pragma warning disable SA1118

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeDiagnosticDescriptors
    {
        // Reserved range 300-319

        private const string _category = "Caravela.DesignTime";

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
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID. "
                    + "Please restart your IDE.",
                    _category,
                    Warning );

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserInfo
                = new(
                    "CR0303",
                    "A Caravela user info.",
                    "{0}: {1} The diagnostic {0} was not defined in the user profile and has been replaced by a generic diagnostic ID. "
                    + " Please restart your IDE.",
                    _category,
                    Info );

        internal static readonly DiagnosticDefinition<(string Id, string Message)>
            UserHidden
                = new(
                    "CR0304",
                    "A Caravela user hidden message.",
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

        internal static readonly DiagnosticDefinition<INamedType>
            TypeNotPartial
                = new(
                    "CR0307",
                    "The type must be made partial.",
                    "Aspects add members to '{0}' but it is not marked as 'partial'. Make the type 'partial' to make it possible to "
                    + "referenced aspect-generated artefacts from source code.",
                    _category,
                    Warning );
    }
}