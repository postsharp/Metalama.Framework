using Xunit.Abstractions;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    internal static class MessageSinkExtensions
    {
        public static void Trace( this IMessageSink messageSink, string message ) => messageSink.OnMessage( new DiagnosticMessage( message ) );
    }
}