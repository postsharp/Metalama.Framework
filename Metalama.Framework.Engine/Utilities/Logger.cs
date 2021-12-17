// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Metalama.Framework.Engine.Utilities
{
    internal class Logger
    {
        private readonly TextWriter _textWriter;
        private static readonly object _initializeSync = new();
        private static volatile Logger? _instance;

        public static Logger? Instance => _instance;

        private Logger( TextWriter textWriter )
        {
            this._textWriter = textWriter;

            this.Write( $"Process={Process.GetCurrentProcess().ProcessName}, CommandLine={Environment.CommandLine}." );
        }

        public static void Initialize()
        {
            if ( _instance == null )
            {
                lock ( _initializeSync )
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if ( _instance == null )
                    {
                        var pid = Process.GetCurrentProcess().Id;

                        var directory = Path.Combine( Path.GetTempPath(), "Metalama", "Logs" );

                        try
                        {
                            RetryHelper.Retry(
                                () =>
                                {
                                    if ( !Directory.Exists( directory ) )
                                    {
                                        Directory.CreateDirectory( directory );
                                    }
                                } );

                            // The filename must be unique because several instances of the current assembly (of different versions) may be loaded in the process.
                            var textWriter = File.CreateText(
                                Path.Combine( directory, $"Metalama.{Process.GetCurrentProcess().ProcessName}.{pid}.{Guid.NewGuid()}.log" ) );

                            _instance = new Logger( textWriter );
                        }
                        catch
                        {
                            // Don't fail if we cannot initialize the log.
                        }
                    }
                }
            }
        }

        public void Write( string s )
        {
            lock ( this._textWriter )
            {
                this._textWriter.WriteLine( $"{DateTime.Now}, Thread {Thread.CurrentThread.ManagedThreadId}: {s}" );
                this._textWriter.Flush();
            }
        }
    }
}