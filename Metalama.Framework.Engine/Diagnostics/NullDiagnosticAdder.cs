// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics
{
    public sealed class NullDiagnosticAdder : IDiagnosticAdder
    {
        private readonly ILogger _logger;

        public static NullDiagnosticAdder Instance { get; } = new();

        private NullDiagnosticAdder()
        {
            this._logger = Logger.LoggerFactory.GetLogger( "NullDiagnosticAdder" );
        }

        void IDiagnosticAdder.Report( Diagnostic diagnostic )
        {
            switch ( diagnostic.Severity )
            {
                case DiagnosticSeverity.Error:
                    this._logger.Error?.Log( diagnostic.ToString() );

                    break;

                case DiagnosticSeverity.Warning:
                    this._logger.Warning?.Log( diagnostic.ToString() );

                    break;
            }
        }
    }
}