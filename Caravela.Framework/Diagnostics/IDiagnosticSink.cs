// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// A sink that reports diagnostics reported from user code.
    /// </summary>
    /// <seealso href="@diagnostics"/>
    [CompileTimeOnly]
    public interface IDiagnosticSink
    {
        /// <summary>
        /// Reports a parameterless diagnostic by specifying its location.
        /// </summary>
        /// <param name="location">The code location to which the diagnostic should be written.</param>
        /// <param name="definition"></param>
        /// <param name="args"></param>
        void Report( IDiagnosticLocation? location, DiagnosticDefinition definition, params object[] args );

        /// <summary>
        /// Reports a parametric diagnostic by specifying its location.
        /// </summary>
        /// <param name="location">The code location to which the diagnostic should be written.</param>
        /// <param name="definition"></param>
        /// <param name="arguments"></param>
        void Report<T>( IDiagnosticLocation? location, DiagnosticDefinition<T> definition, T arguments )
            where T : notnull;

        /// <summary>
        /// Suppresses a diagnostic by specifying the declaration in which the suppression must be effective.
        /// </summary>
        /// <param name="scope">The declaration in which the diagnostic must be suppressed.</param>
        /// <param name="definition"></param>
        void Suppress( IDeclaration? scope, SuppressionDefinition definition );
    }
}