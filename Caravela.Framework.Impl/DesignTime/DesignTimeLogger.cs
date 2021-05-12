// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class DesignTimeLogger
    {
        public static DesignTimeLogger? Instance { get; } = new();

        private readonly TextWriter _textWriter;

        private DesignTimeLogger()
        {
            var pid = Process.GetCurrentProcess().Id;

            this._textWriter = File.CreateText(
                Path.Combine( Path.GetTempPath(), $"Caravela.Framework.DesignTime.{Process.GetCurrentProcess().ProcessName}.{pid}.log" ) );

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