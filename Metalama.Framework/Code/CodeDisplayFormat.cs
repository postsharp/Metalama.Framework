// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

#pragma warning disable SA1623

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Defines the formatting options of <see cref="IDisplayable.ToDisplayString"/>. Only well-known instances of this classes,
    /// exposed as properties, are currently supported.
    /// </summary>
    [CompileTimeOnly]
    public sealed class CodeDisplayFormat
    {
        // Prevents creation of custom instances.
        private CodeDisplayFormat() { }

        /// <summary>
        /// Emits fully-qualified code references, including namespaces and aliases.
        /// </summary>
        public static CodeDisplayFormat FullyQualified { get; } = new();

        /// <summary>
        /// Formats code references as in a C# error message.
        /// </summary>
        public static CodeDisplayFormat DiagnosticMessage { get; } = new();

        /// <summary>
        /// Emits minimally-qualified code references.
        /// </summary>
        public static CodeDisplayFormat MinimallyQualified { get; } = new();

        /// <summary>
        /// Formats code references as in a C# short error message.
        /// </summary>
        public static CodeDisplayFormat ShortDiagnosticMessage { get; } = new();
    }
}