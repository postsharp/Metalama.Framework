// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.CodeFixes;
using System;

namespace Caravela.Framework.Diagnostics
{
    /// <summary>
    /// Defines extension methods for <see cref="IDiagnosticSink"/>.
    /// </summary>
    /// <seealso href="@diagnostics"/>
    [CompileTimeOnly]
    public static class DiagnosticSinkExtensions
    {
        /// <summary>
        /// Reports a parameterless diagnostic by specifying its target declaration.
        /// </summary>
        /// <param name="diagnosticSink"></param>
        /// <param name="scope">The target declaration of the diagnostic (typically an <see cref="IDeclaration"/>). If null, the location of the current target is used. </param>
        /// <param name="definition">A <see cref="DiagnosticDefinition"/>, which must be defined in a static field or property of an aspect class.</param>
        public static void Report(
            this IDiagnosticSink diagnosticSink,
            IDiagnosticScope? scope,
            DiagnosticDefinition definition,
            Action<ICodeFixProviderContext>? codeFixProvider = null )
            => diagnosticSink.Report( scope?.DiagnosticLocation, definition, codeFixProvider );

        /// <summary>
        /// Reports a diagnostic by specifying its target declaration.
        /// </summary>
        /// <param name="diagnosticSink"></param>
        /// <param name="scope">The target declaration of the diagnostic (typically an <see cref="IDeclaration"/>). If null, the location of the current target is used. </param>
        /// <param name="definition">A <see cref="DiagnosticDefinition"/>, which must be defined in a static field or property of an aspect class.</param>
        /// <param name="arguments">Arguments of the formatting string (typically a single value or a tuple).</param>
        public static void Report<T>(
            this IDiagnosticSink diagnosticSink,
            IDiagnosticScope? scope,
            DiagnosticDefinition<T> definition,
            T arguments,
            Action<ICodeFixProviderContext>? codeFixProvider = null )
            where T : notnull
            => diagnosticSink.Report( scope?.DiagnosticLocation, definition, arguments, codeFixProvider );

        /// <summary>
        /// Reports a parameterless diagnostic on the current target declaration.
        /// </summary>
        /// <param name="diagnosticSink"></param>
        /// <param name="definition">A <see cref="DiagnosticDefinition"/>, which must be defined in a static field or property of an aspect class.</param>
        public static void Report(
            this IDiagnosticSink diagnosticSink,
            DiagnosticDefinition definition,
            Action<ICodeFixProviderContext>? codeFixProvider = null )
            => diagnosticSink.Report( null, definition, codeFixProvider );

        /// <summary>
        /// Reports a strongly-typed diagnostic on the current target declaration.
        /// </summary>
        /// <param name="diagnosticSink"></param> 
        /// <param name="definition">A <see cref="DiagnosticDefinition"/>, which must be defined in a static field or property of an aspect class.</param>
        /// <param name="arguments">Arguments of the formatting string (typically a single value or a tuple).</param>
        public static void Report<T>(
            this IDiagnosticSink diagnosticSink,
            DiagnosticDefinition<T> definition,
            T arguments,
            Action<ICodeFixProviderContext>? codeFixProvider = null )
            where T : notnull
            => diagnosticSink.Report( null, definition, arguments, codeFixProvider );

        /// <summary>
        /// Suppresses a diagnostic in the current target declaration.
        /// </summary>
        /// <param name="diagnosticSink"></param> 
        /// <param name="definition">A <see cref="SuppressionDefinition"/>, which must be defined in a static field or property of an aspect class.</param>
        public static void Suppress( this IDiagnosticSink diagnosticSink, SuppressionDefinition definition ) => diagnosticSink.Suppress( null, definition );
    }
}