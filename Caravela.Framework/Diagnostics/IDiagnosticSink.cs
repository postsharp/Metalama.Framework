// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.CodeFixes;
using Caravela.Framework.Validation;
using System.Collections.Generic;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// A sink that reports diagnostics reported from user code.
    /// </summary>
    /// <seealso href="@diagnostics"/>
    [CompileTimeOnly]
    [InternalImplement]
    public interface IDiagnosticSink
    {
        /// <summary>
        /// Reports a parametric diagnostic by specifying its location.
        /// </summary>
        /// <param name="location">The code location to which the diagnostic should be written, typically an <see cref="IDeclaration"/>.</param>
        /// <param name="definition">The diagnostic definition, which must be defined as a static field or property.</param>
        /// <param name="arguments">The diagnostic arguments.</param>
        /// <param name="codeFixes">An optional <see cref="CodeFix"/> for the diagnostic, or a collection of code fixes. Note that the <see cref="CodeFix"/> class
        /// implements the <c>IEnumerable&lt;CodeFix&gt;</c> so you don't need to use additional syntax for a single code fix.</param>
        void Report<T>(
            IDiagnosticLocation location,
            DiagnosticDefinition<T> definition,
            T arguments,
            IEnumerable<CodeFix>? codeFixes = null )
            where T : notnull;

        /// <summary>
        /// Suppresses a diagnostic by specifying the declaration in which the suppression must be effective.
        /// </summary>
        /// <param name="scope">The declaration in which the diagnostic must be suppressed.</param>
        /// <param name="definition">The suppression definition, which must be defined as a static field or property.</param>
        void Suppress( IDeclaration scope, SuppressionDefinition definition );

        /// <summary>
        /// Suggest a code fix without reporting a diagnostic.
        /// </summary>
        /// <param name="location">The code location for which the code fix should be suggested, typically an <see cref="IDeclaration"/>.</param>
        /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
        void Suggest( IDiagnosticLocation location, CodeFix codeFix );
    }
}