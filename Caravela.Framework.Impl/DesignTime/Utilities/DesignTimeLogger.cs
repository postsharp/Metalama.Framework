// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.Utilities
{
    internal class DesignTimeLogger
    {
        private readonly TextWriter _textWriter;

        public static DesignTimeLogger? Instance { get; } = new();

        private DesignTimeLogger()
        {
            // TODO: Move to Microsoft.Extensions.Logging.

            var pid = Process.GetCurrentProcess().Id;

            var directory = Path.Combine( Path.GetTempPath(), "Caravela", "Logs" );

            RetryHelper.Retry(
                () =>
                {
                    if ( !Directory.Exists( directory ) ) { Directory.CreateDirectory( directory ); }
                } );

            this._textWriter = File.CreateText(
                Path.Combine( directory, $"Caravela.Framework.DesignTime.{Process.GetCurrentProcess().ProcessName}.{pid}.log" ) );

            this.Write( $"Process={Process.GetCurrentProcess().ProcessName}, CommandLine={Environment.CommandLine}." );
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