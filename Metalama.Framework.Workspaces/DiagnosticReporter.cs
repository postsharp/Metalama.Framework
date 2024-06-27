using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Workspaces;

public static class DiagnosticReporter
{
    public static int Warnings { get; private set; }

    public static Action<IIntrospectionDiagnostic>? ReportAction { get; set; } = d => Console.WriteLine( d.FormatAsBuildDiagnostic() );

    public static IEnumerable<IIntrospectionDiagnostic> Report(this IEnumerable<IIntrospectionReference> references, Severity severity, string id, string message)
        => references
            .SelectMany(r => r.Details)
            .Select(r => new DiagnosticTarget( r.Source.GetDiagnosticLocation(), r.Reference.OriginDeclaration, r ) )
            .GroupBy(r => r.Location?.GetLineSpan().StartLinePosition.Line ) // Report a single warning per line.
            .Select(g =>
            {
                var items = g.ToReadOnlyList();

                return new DiagnosticTarget( items[0].Location, items[0].Declaration, items.Select( i => i.Details ).ToArray() );
            } )
            .Report(severity, id, message);

    public static IEnumerable<IIntrospectionDiagnostic> Report(this IEnumerable<IDeclaration> declarations, Severity severity,string id, string message)
        => declarations
            .Select(x => new DiagnosticTarget( x.GetDiagnosticLocation(), x, null))
            .Report(severity, id, message);

    record DiagnosticTarget( Location? Location, IDeclaration Declaration, object? Details );
    private static IEnumerable<IIntrospectionDiagnostic> Report(this IEnumerable<DiagnosticTarget> targets, Severity severity, string id, string message)
    {
        foreach (var location in targets)
        {
            Warnings++;

            var diagnostic = new UserDiagnostic(
                severity,
                id,
                message,
                location.Location.SourceTree?.FilePath,
                location.Location.GetLineSpan().StartLinePosition.Line + 1,
                location.Declaration,
                location.Details );
            
            ReportAction?.Invoke( diagnostic );
            
            
        }
    }
}