// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Defines the formatting options of <see cref="IDisplayable.ToDisplayString"/>. Only well-known instances of this classes,
    /// exposed as properties, are currently supported.
    /// </summary>
    [CompileTimeOnly]
    public sealed class CodeDisplayFormat
    {

        // Prevents creation of custom instances.
        private CodeDisplayFormat()
        {
        }

        public static CodeDisplayFormat FullyQualified { get; } = new CodeDisplayFormat();

        public static CodeDisplayFormat DiagnosticMessage { get; } = new CodeDisplayFormat();

        public static CodeDisplayFormat MinimallyQualified { get; } = new CodeDisplayFormat();

        public static CodeDisplayFormat ShortDiagnosticMessage { get; } = new CodeDisplayFormat();
    }
}