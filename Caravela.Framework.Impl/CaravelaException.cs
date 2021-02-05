﻿using System;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class CaravelaException : Exception
    {
        public Diagnostic Diagnostic { get; }

        public CaravelaException( DiagnosticDescriptor diagnosticDescriptor, params object[] args )
            : this( diagnosticDescriptor, null, args )
        {
        }

        public CaravelaException( DiagnosticDescriptor diagnosticDescriptor, Location? location, params object[] args )
            : this( Diagnostic.Create( diagnosticDescriptor, location, args ) )
        {
        }

        private CaravelaException( Diagnostic diagnostic )
            : base( diagnostic.ToString() )
            => this.Diagnostic = diagnostic;
    }

    internal static class ExceptionExtensions
    {
        public static string ToDiagnosticString( this Exception ex ) =>
            ex.InnerException == null ? $"{ex.GetType()}: {ex.Message}" : $"{ex.GetType()}: {ex.Message} -> {ex.InnerException.ToDiagnosticString()}";
    }
}
