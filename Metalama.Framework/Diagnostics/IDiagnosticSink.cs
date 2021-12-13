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
    [CompileTimeOnly]
    [InternalImplement]
    public interface IDiagnosticSink
    {
        /// <summary>
        /// Reports a parametric diagnostic by specifying its location.
        /// </summary>
        /// <param name="diagnostic"></param>
        void Report( IDiagnosticLocation? location, IDiagnostic diagnostic );

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

    public readonly struct ScopedDiagnosticSink : IDiagnosticSink
    {
        private readonly IDiagnosticSink _sink;
        private readonly IDeclaration _declaration;
        private readonly IDiagnosticLocation _location;

        internal ScopedDiagnosticSink( IDiagnosticSink sink, IDiagnosticLocation location, IDeclaration declaration )
        {
            this._sink = sink;
            this._location = location;
            this._declaration = declaration;
        }

        /// <summary>
        /// Reports a parametric diagnostic by specifying its location.
        /// </summary>
        /// <param name="diagnostic"></param>
        public void Report( IDiagnostic diagnostic ) => this._sink.Report( this._location, diagnostic );

        /// <summary>
        /// Suppresses a diagnostic by specifying the declaration in which the suppression must be effective.
        /// </summary>
        /// <param name="definition">The suppression definition, which must be defined as a static field or property.</param>
        public void Suppress( SuppressionDefinition definition ) => this._sink.Suppress( this._declaration, definition );

        /// <summary>
        /// Suggest a code fix without reporting a diagnostic.
        /// </summary>
        /// <param name="codeFix">The <see cref="CodeFix"/>.</param>
        public void Suggest( CodeFix codeFix ) => this._sink.Suggest( this._declaration, codeFix );

        public void Report( IDiagnosticLocation? location, IDiagnostic diagnostic ) => this._sink.Report( location, diagnostic );

        public void Suppress( IDeclaration scope, SuppressionDefinition definition ) => this._sink.Suppress( scope, definition );

        public void Suggest( IDiagnosticLocation location, CodeFix codeFix ) => this._sink.Suggest( location, codeFix );
    }
}