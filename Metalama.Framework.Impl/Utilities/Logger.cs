// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Metalama.Framework.Impl.Utilities
{
    internal class Logger
    {
        private readonly TextWriter _textWriter;

        public static Logger? Instance { get; private set; }

        private Logger( TextWriter textWriter )
        {
            this._textWriter = textWriter;

            this.Write( $"Process={Process.GetCurrentProcess().ProcessName}, CommandLine={Environment.CommandLine}." );
        }

        public static void Initialize( TextWriter textWriter )
        {
            Instance = new Logger( textWriter );
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