// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

#pragma warning disable SA1623

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Defines the formatting options of <see cref="IDisplayable.ToDisplayString"/>. Only well-known instances of this classes,
    /// exposed as properties, are currently supported.
    /// </summary>
    [CompileTime]
    public sealed class CodeDisplayFormat
    {
        // Prevents creation of custom instances.
        private CodeDisplayFormat( bool includeParent )
        {
            this.IncludeParent = includeParent;
        }

        /// <summary>
        /// Emits fully-qualified code references, including namespaces and aliases.
        /// </summary>
        public static CodeDisplayFormat FullyQualified { get; } = new( true );

        /// <summary>
        /// Formats code references as in a C# error message.
        /// </summary>
        public static CodeDisplayFormat DiagnosticMessage { get; } = new( true );

        /// <summary>
        /// Emits minimally-qualified code references.
        /// </summary>
        public static CodeDisplayFormat MinimallyQualified { get; } = new( false );

        /// <summary>
        /// Formats code references as in a C# short error message.
        /// </summary>
        public static CodeDisplayFormat ShortDiagnosticMessage { get; } = new( true );

        internal bool IncludeParent { get; }
    }
}