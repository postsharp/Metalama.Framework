// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.IO;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Metalama.Testing.AspectTesting.XunitFramework
{
    internal sealed class TestOutputHelper : ITestOutputHelper
    {
        private readonly IMessageSink _messageSink;
        private readonly ITest _test;

        public TestOutputHelper( IMessageSink messageSink, ITest test )
        {
            this._messageSink = messageSink;
            this._test = test;
        }

        public void WriteLine( string message ) => this._messageSink.OnMessage( new TestOutput( this._test, message + Environment.NewLine ) );

        public void WriteLine( string format, params object[] args ) => this.WriteLine( string.Format( format, args ) );
    }
}