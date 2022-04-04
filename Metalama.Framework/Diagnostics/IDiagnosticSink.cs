// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Diagnostics
{
    /// <summary>
    /// A sink that reports diagnostics reported from user code.
    /// </summary>
    /// <seealso href="@diagnostics"/>
    [CompileTime]
    [InternalImplement]
    public interface IDiagnosticSink
    {
        /// <summary>
        /// Reports a parametric diagnostic by specifying its location.
        /// </summary>
        void Report( IDiagnostic diagnostic, IDiagnosticLocation? location );

        /// <summary>
        /// Suppresses a diagnostic by specifying the declaration in which the suppression must be effective.
        /// </summary>
        /// <param name="suppression">The suppression definition, which must be defined as a static field or property.</param>
        /// <param name="scope">The declaration in which the diagnostic must be suppressed.</param>
        void Suppress( SuppressionDefinition suppression, IDeclaration scope );

        /// <summary>
        /// Suggest a code fix without reporting a diagnostic.
        /// </summary>
        /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
        /// <param name="location">The code location for which the code fix should be suggested, typically an <see cref="IDeclaration"/>.</param>
        void Suggest( CodeFix codeFix, IDiagnosticLocation location );
    }
}