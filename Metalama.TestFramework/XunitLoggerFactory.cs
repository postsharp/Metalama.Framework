﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System.Collections.Concurrent;
using Xunit.Abstractions;

namespace Metalama.TestFramework;

public class XunitLoggerFactory : ILoggerFactory
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ConcurrentDictionary<string, Logger> _loggers = new();

    public XunitLoggerFactory( ITestOutputHelper testOutputHelper )
    {
        this._testOutputHelper = testOutputHelper;
    }

    public void Dispose() { }

    public ILogger GetLogger( string category ) => this._loggers.GetOrAdd( category, c => new Logger( c, this._testOutputHelper ) );

    private class Logger : ILogger
    {
        private readonly string _prefix;
        private readonly ITestOutputHelper _testOutputHelper;

        public Logger( string prefix, ITestOutputHelper testOutputHelper )
        {
            this._prefix = prefix;
            this._testOutputHelper = testOutputHelper;
            this.Trace = new LogWriter( prefix, "TRACE", testOutputHelper );
            this.Info = new LogWriter( prefix, "INFO", testOutputHelper );
            this.Warning = new LogWriter( prefix, "WARNING", testOutputHelper );
            this.Error = new LogWriter( prefix, "ERROR", testOutputHelper );
        }

        ILogger ILogger.WithPrefix( string prefix ) => new Logger( this._prefix + "." + prefix, this._testOutputHelper );

        public ILogWriter? Trace { get; }

        public ILogWriter? Info { get; }

        public ILogWriter? Warning { get; }

        public ILogWriter? Error { get; }
    }

    private class LogWriter : ILogWriter
    {
        private readonly string _prefix;
        private readonly string _severity;
        private readonly ITestOutputHelper _testOutputHelper;

        public LogWriter( string prefix, string severity, ITestOutputHelper testOutputHelper )
        {
            this._prefix = prefix;
            this._severity = severity;
            this._testOutputHelper = testOutputHelper;
        }

        public void Log( string message ) => this._testOutputHelper.WriteLine( $"{this._severity} {this._prefix}: {message}" );
    }
}