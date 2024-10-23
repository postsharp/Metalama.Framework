// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.IO;

namespace Metalama.Framework.Engine;

#pragma warning disable RS1001 // Missing diagnostic analyzer attribute

internal class GeneratedCodeAnalyzer : DiagnosticAnalyzer
{
    private const string _diagnosticCategory = "Metalama.GeneratedCodeAnalyzer";

    internal static readonly DiagnosticDefinition<(string AspectType, ISymbol Target, string Addendum)> AspectAppliedToGeneratedCode = new(
        "LAMA0320",
        "Aspect can't be applied to source generated code.",
        "The aspect '{0}' can't be applied to '{1}', because it's in source generated code.{2}",
        _diagnosticCategory,
        Severity.Warning );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create( AspectAppliedToGeneratedCode.ToRoslynDescriptor() );

    public override void Initialize( AnalysisContext context )
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics );

        context.RegisterSymbolAction( this.AnalyzeSymbol, SymbolKind.Event, SymbolKind.Field, SymbolKind.Method, SymbolKind.NamedType, SymbolKind.Parameter, SymbolKind.Property );
    }

    private void AnalyzeSymbol( SymbolAnalysisContext context )
    {
#if ROSLYN_4_4_0_OR_GREATER // Roslyn 4.0 doesn't have IsGeneratedCode, so just do nothing in that case.

        // IsGeneratedCode is based on heuristics, so it's not going to be exactly the same as files produced by source generators, but I can't think of a better way to do this.
        if ( !context.IsGeneratedCode )
        {
            return;
        }

        var iAspect = context.Compilation.GetTypeByMetadataName( typeof( IAspect ).FullName! );

        var symbol = context.Symbol;

        foreach ( var attribute in symbol.GetAttributes() )
        {
            if ( context.Compilation.HasImplicitConversion( attribute.AttributeClass, iAspect ) )
            {
                var location = attribute.GetDiagnosticLocation() ?? symbol.GetDiagnosticLocation();

                var addendum = string.Empty;

                if ( location != null && Path.GetExtension( location.GetMappedLineSpan().Path ) is ".razor" or ".cshtml" )
                {
                    addendum = " For Razor files, consider extracting the relevant code to code behind.";
                }

                var diagnostic = AspectAppliedToGeneratedCode.CreateRoslynDiagnostic(
                    location,
                    (AttributeHelper.GetShortName( attribute.AttributeClass!.MetadataName ), symbol, addendum) );

                context.ReportDiagnostic( diagnostic );
            }
        }
#endif
    }
}