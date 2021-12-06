// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using PostSharp.Backstage.Extensibility;
using System;

namespace Caravela.Framework.Tests.UnitTests.Utilities
{
    /// <summary>
    /// Diagnostics sink throwing <see cref="InvalidOperationException"/> for any diagnostic.
    /// </summary>
    internal class ThrowingDiagnosticsSink : IBackstageDiagnosticSink, IService
    {
        /// <inheritdoc />
        public void ReportWarning( string message, IDiagnosticsLocation? location = null ) => throw new InvalidOperationException( message );

        /// <inheritdoc />
        public void ReportError( string message, IDiagnosticsLocation? location = null ) => throw new InvalidOperationException( message );
    }
}