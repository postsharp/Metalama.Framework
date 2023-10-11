// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using Backstage = Metalama.Backstage.Diagnostics;
using Compiler = Metalama.Compiler.Services;

namespace Metalama.Framework.Engine.Services;

internal class LoggerAdapter : Compiler::ILogger
{
    public LoggerAdapter( Backstage::ILogger backstageLogger )
    {
        this.Trace = CreateWriter( () => backstageLogger.Trace );
        this.Info = CreateWriter( () => backstageLogger.Info );
        this.Warning = CreateWriter( () => backstageLogger.Warning );
        this.Error = CreateWriter( () => backstageLogger.Error );

        static Compiler::ILogWriter CreateWriter( Func<Backstage::ILogWriter?> getBackstageWriter ) => new LogWriterAdapter( getBackstageWriter );
    }

    public Compiler::ILogWriter Trace { get; set; }

    public Compiler::ILogWriter Info { get; set; }

    public Compiler::ILogWriter Warning { get; set; }

    public Compiler::ILogWriter Error { get; set; }
}