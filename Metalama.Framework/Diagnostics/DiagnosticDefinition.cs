// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.CodeFixes;
using System.Collections.Immutable;

namespace Metalama.Framework.Diagnostics
{
    // ReSharper disable once UnusedTypeParameter

    /// <summary>
    /// Defines a diagnostic that does not accept any parameters. For a diagnostic that accepts parameters, use <see cref="DiagnosticDefinition{T}"/>.
    /// </summary>
    /// <seealso href="@diagnostics"/>
    public sealed class DiagnosticDefinition : DiagnosticDefinition<None>, IDiagnostic
    {
        public DiagnosticDefinition( string id, Severity severity, string messageFormat, string? title = null, string? category = null ) : base(
            id,
            severity,
            messageFormat,
            title,
            category ) { }

        IDiagnosticDefinition IDiagnostic.Definition => this;

        ImmutableArray<CodeFix> IDiagnostic.CodeFixes => ImmutableArray<CodeFix>.Empty;

        object? IDiagnostic.Arguments => default(None);

        public IDiagnostic WithCodeFixes( params CodeFix[] codeFixes ) => new DiagnosticImpl<None>( this, default, codeFixes.ToImmutableArray() );

        /// <summary>
        /// Reports the current diagnostic to a given <see cref="IDiagnosticLocation"/> (typically a declaration or syntax node). 
        /// </summary>
        public void ReportTo( IDiagnosticLocation location, IDiagnosticSink sink ) => sink.Report( location, this );

        /// <summary>
        /// Reports the current diagnostic to the default location (declaration or syntax node) of the current context.
        /// </summary>
        /// <param name="sink">The <see cref="ScopedDiagnosticSink"/> for the current context.</param>
        public void ReportTo( in ScopedDiagnosticSink sink ) => sink.Report( this );
    }
}