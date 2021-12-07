// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Xunit.Abstractions;
using Xunit.Sdk;

namespace Metalama.TestFramework
{
    internal static class MessageSinkExtensions
    {
        public static void Trace( this IMessageSink messageSink, string message ) => messageSink.OnMessage( new DiagnosticMessage( message ) );
    }
}