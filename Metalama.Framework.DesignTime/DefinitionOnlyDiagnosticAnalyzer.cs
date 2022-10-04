// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime;

#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

public class DefinitionOnlyDiagnosticAnalyzer : DiagnosticAnalyzer
{
    private protected DesignTimeDiagnosticDefinitions DiagnosticDefinitions { get; } = DesignTimeDiagnosticDefinitions.GetInstance();

    static DefinitionOnlyDiagnosticAnalyzer()
    {
        DesignTimeServices.Initialize();
    }

    public override void Initialize( AnalysisContext context )
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.None );
    }

    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => this.DiagnosticDefinitions.SupportedDiagnosticDescriptors.Values.ToImmutableArray();
}