// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;

namespace Metalama.Framework.DesignTime.VisualStudio;

#pragma warning disable RS1001

[UsedImplicitly]
public class VsUserProcessDiagnosticAnalyzer : DefinitionOnlyDiagnosticAnalyzer
{
    // This class exists only because the this.SupportedDiagnostics member is called.
    // It is required for code fixes. If this implementation does not run in devenv, the CodeFixProvider is not called.
}