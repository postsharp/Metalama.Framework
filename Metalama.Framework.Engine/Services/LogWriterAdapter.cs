// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using Backstage = Metalama.Backstage.Diagnostics;
using Compiler = Metalama.Compiler.Services;

namespace Metalama.Framework.Engine.Services;

internal class LogWriterAdapter : Compiler::ILogWriter
{
    private readonly Func<Backstage::ILogWriter?> _getBackstageWriter;

    public LogWriterAdapter( Func<Backstage::ILogWriter?> getBackstageWriter )
    {
        this._getBackstageWriter = getBackstageWriter;
    }

    public void Log( string message ) => this._getBackstageWriter()?.Log( message );
}