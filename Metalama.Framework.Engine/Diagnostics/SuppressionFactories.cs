// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Diagnostics;

public class SuppressionFactories
{
    public static SuppressionDescriptor CreateDescriptor( string diagnosticId )
        => new( "Metalama." + diagnosticId, diagnosticId, justification: string.Empty );

    public static SuppressionDiagnostic CreateDiagnostic( Diagnostic diagnostic, CancellationToken cancellationToken = default )
    {
        var recorder = new RecorderFormatProvider();
        diagnostic.GetMessage( recorder );
        var args = recorder.Arguments;
        var invariantMessage = diagnostic.GetMessage( CultureInfo.InvariantCulture );

        SourceSpan? span = null;

        if ( diagnostic.Location.SourceTree is { } tree )
        {
            var sourceSpan = diagnostic.Location.SourceSpan;
            var lineSpan = tree.GetLineSpan( sourceSpan, cancellationToken );
            var text = tree.GetText( cancellationToken ).ToString( sourceSpan );

            span = new SourceSpan(
                tree.FilePath,
                tree,
                sourceSpan.Start,
                sourceSpan.End,
                lineSpan.StartLinePosition.Line,
                lineSpan.StartLinePosition.Character,
                lineSpan.EndLinePosition.Line,
                lineSpan.EndLinePosition.Character,
                text );
        }

        return new( diagnostic.Id, invariantMessage, args.ToImmutableArray(), span );
    }

    private sealed class RecorderFormatProvider : IFormatProvider, ICustomFormatter
    {
        public List<object?> Arguments { get; } = [];

        public object? GetFormat( Type? formatType )
        {
            if ( formatType == typeof(ICustomFormatter) )
            {
                return this;
            }

            return null;
        }

        public string Format( string? format, object? arg, IFormatProvider? formatProvider )
        {
            this.Arguments.Add( arg );

            return string.Empty;
        }
    }
}