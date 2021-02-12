using System;

namespace Caravela.Framework.Impl
{
    internal static class ExceptionExtensions
    {
        public static string ToDiagnosticString( this Exception ex ) =>
            ex.InnerException == null ? $"{ex.GetType()}: {ex.Message}" : $"{ex.GetType()}: {ex.Message} -> {ex.InnerException.ToDiagnosticString()}";
    }
}
