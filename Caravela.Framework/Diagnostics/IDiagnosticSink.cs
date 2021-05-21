// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Project;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// A sink that reports diagnostics reported from user code.
    /// </summary>
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
        void Report<T>( IDiagnosticLocation? location, DiagnosticDefinition<T> definition, T arguments );

        /// <summary>
        /// Suppresses a diagnostic by specifying the element of code in which the suppression must be effective.
        /// </summary>
        /// <param name="scope">The code element in which the diagnostic must be suppressed.</param>
        /// <param name="definition"></param>
        void Suppress( IDeclaration? scope, SuppressionDefinition definition );
    }
}