// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using Backstage = Metalama.Backstage.Telemetry;
using Compiler = Metalama.Compiler.Services;

namespace Metalama.Framework.Engine.Services;

internal sealed class ExceptionReporterAdapter : Compiler::IExceptionReporter
{
    private readonly Backstage::IExceptionReporter? _backstageReporter;

    public ExceptionReporterAdapter( Backstage::IExceptionReporter? backstageReporter )
    {
        this._backstageReporter = backstageReporter;
    }

    public void ReportException( Exception reportedException ) => this._backstageReporter?.ReportException( reportedException );
}