// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.IO;
using Xunit.Abstractions;

namespace Metalama.TestFramework.XunitFramework
{
    internal class TestOutputHelper : ITestOutputHelper
    {
        public StringWriter StringWriter { get; } = new();

        public void WriteLine( string message ) => this.StringWriter.WriteLine( message );

        public void WriteLine( string format, params object[] args ) => this.StringWriter.WriteLine( format, args );

        public override string ToString() => this.StringWriter.ToString();
    }
}