// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using PostSharp.Backstage.Extensibility;
using System;

namespace Caravela.Framework.Tests.UnitTests.Utilities
{
    internal class ThrowingDiagnosticsSink : IDiagnosticsSink, IService
    {
        public void ReportWarning( string message, IDiagnosticsLocation? location = null ) => throw new InvalidOperationException( message );

        public void ReportError( string message, IDiagnosticsLocation? location = null ) => throw new InvalidOperationException( message );
    }
}